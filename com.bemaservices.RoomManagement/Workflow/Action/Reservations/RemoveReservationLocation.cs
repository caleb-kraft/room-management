// <copyright>
// Copyright by BEMA Software Services
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using com.bemaservices.RoomManagement.Model;
using com.bemaservices.RoomManagement.Attribute;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Workflow;

namespace com.bemaservices.RoomManagement.Workflow.Actions.Reservations
{
    /// <summary>
    /// Removes a location from a reservation.
    /// </summary>
    [ActionCategory( "Room Management" )]
    [Description( "Removes a location from a reservation. Optionally removes or reassigns resources assigned to that location." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Remove Location" )]

    [WorkflowAttribute( "Reservation Attribute", "The attribute that contains the reservation.", true, "", "", 0, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType" } )]

    [WorkflowAttribute( "Location Attribute", "The attribute that contains the location to remove.", false, "", "", 1, null,
        new string[] { "Rock.Field.Types.LocationFieldType" } )]

    [BooleanField( "Remove Assigned Resources", "If true, resources assigned to this location will also be removed. If false, resources will be kept but unassigned from the location.", true, "", 2 )]

    public class RemoveReservationLocation : ActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns><c>true</c> if the action executed successfully, <c>false</c> otherwise.</returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();
            var reservationService = new ReservationService( rockContext );
            var reservationLocationService = new ReservationLocationService( rockContext );
            var reservationResourceService = new ReservationResourceService( rockContext );

            // Get the reservation
            Reservation reservation = null;
            Guid reservationGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "ReservationAttribute" ).AsGuid() ).AsGuid();
            reservation = reservationService.Get( reservationGuid );
            if ( reservation == null )
            {
                errorMessages.Add( "Invalid Reservation Attribute or Value!" );
                return false;
            }

            // Get the location
            Location location = null;
            var locationAttributeGuid = GetAttributeValue( action, "LocationAttribute" ).AsGuidOrNull();
            if ( locationAttributeGuid.HasValue )
            {
                var locationGuid = action.GetWorkflowAttributeValue( locationAttributeGuid.Value ).AsGuidOrNull();
                if ( locationGuid.HasValue && locationGuid.Value != Guid.Empty )
                {
                    location = new LocationService( rockContext ).Get( locationGuid.Value );
                }
            }
            
            if ( location == null )
            {
                errorMessages.Add( "Invalid Location Attribute or Value!" );
                return false;
            }

            // Find the reservation location
            var reservationLocation = reservation.ReservationLocations.FirstOrDefault( rl => rl.LocationId == location.Id );
            if ( reservationLocation == null )
            {
                errorMessages.Add( String.Format( "Location '{0}' is not assigned to this reservation.", location.Name ) );
                return false;
            }

            var oldApprovalState = reservation.ApprovalState;
            var changes = new History.HistoryChangeList();
            var locationName = reservationLocation.Location?.Name ?? location.Name;

            // Handle resources assigned to this location
            bool removeAssignedResources = GetAttributeValue( action, "RemoveAssignedResources" ).AsBoolean( true );
            var assignedResources = reservation.ReservationResources.Where( rr => rr.ReservationLocationId == reservationLocation.Id ).ToList();
            
            if ( assignedResources.Any() )
            {
                if ( removeAssignedResources )
                {
                    // Remove the resources
                    foreach ( var resource in assignedResources )
                    {
                        changes.Add( new History.HistoryChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, 
                            String.Format( "[Resource] {0} {1} (removed with location)", resource.Quantity.HasValue ? resource.Quantity.ToString() : "", resource.Resource?.Name ?? "Unknown" ) ) );
                        reservationResourceService.Delete( resource );
                    }
                }
                else
                {
                    // Unassign resources from location (set ReservationLocationId to null)
                    foreach ( var resource in assignedResources )
                    {
                        resource.ReservationLocationId = null;
                        changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, 
                            String.Format( "[Resource] {0} {1} (unassigned from location)", resource.Quantity.HasValue ? resource.Quantity.ToString() : "", resource.Resource?.Name ?? "Unknown" ) ) );
                    }
                }
            }

            // Remove the location
            changes.Add( new History.HistoryChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, String.Format( "[Location] {0}", locationName ) ) );
            reservationLocationService.Delete( reservationLocation );

            // Update approval state if needed
            // If this was the only location and it's removed, reservation might need state change
            if ( !reservation.ReservationLocations.Any( rl => rl.Id != reservationLocation.Id ) )
            {
                // No locations remaining - reservation might need to be cancelled or changed
                if ( reservation.ApprovalState == ReservationApprovalState.Approved || 
                     reservation.ApprovalState == ReservationApprovalState.PendingInitialApproval ||
                     reservation.ApprovalState == ReservationApprovalState.PendingFinalApproval )
                {
                    reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                    History.EvaluateChange(
                        changes,
                        "Approval State",
                        oldApprovalState.ToString(),
                        reservation.ApprovalState.ToString() );
                }
            }
            else
            {
                // Recalculate approval state based on remaining locations/resources
                reservation = reservationService.UpdateApproval( reservation, reservation.ApprovalState, false );
                if ( oldApprovalState != reservation.ApprovalState )
                {
                    History.EvaluateChange(
                        changes,
                        "Approval State",
                        oldApprovalState.ToString(),
                        reservation.ApprovalState.ToString() );
                }
            }

            // Save reservation changes first to ensure reservation.Id and related entities are persisted
            rockContext.SaveChanges();

            if ( changes.Any() )
            {
                changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, string.Format( "Updated by the '{0}' workflow", action.ActionTypeCache.ActivityType.WorkflowType.Name ) ) );
                HistoryService.SaveChanges( rockContext, typeof( Reservation ), com.bemaservices.RoomManagement.SystemGuid.Category.HISTORY_RESERVATION_CHANGES.AsGuid(), reservation.Id, changes, false );
            }

            return true;
        }
    }
}
