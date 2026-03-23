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
    /// Removes a resource from a reservation or reduces its quantity.
    /// </summary>
    [ActionCategory( "Room Management" )]
    [Description( "Removes a resource from a reservation. If the resource has a quantity and a quantity is specified, reduces the quantity instead of removing entirely." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Remove Resource" )]

    [WorkflowAttribute( "Reservation Attribute", "The attribute that contains the reservation.", true, "", "", 0, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType" } )]

    [WorkflowAttribute( "Resource Attribute", "The attribute that contains the resource to remove.", false, "", "", 1, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ResourceFieldType" } )]

    [WorkflowTextOrAttribute( "Quantity To Remove", "Attribute Value", "The quantity to remove (for quantity-based resources). If not specified or 0, removes the resource entirely. <span class='tip tip-lava'></span>",
        false, "", "", 2, "QuantityToRemove", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    public class RemoveReservationResource : ActionComponent
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
            var reservationResourceService = new ReservationResourceService( rockContext );
            var mergeFields = GetMergeFields( action );

            // Get the reservation
            Reservation reservation = null;
            Guid reservationGuid = action.GetWorkflowAttributeValue( GetAttributeValue( action, "ReservationAttribute" ).AsGuid() ).AsGuid();
            reservation = reservationService.Get( reservationGuid );
            if ( reservation == null )
            {
                errorMessages.Add( "Invalid Reservation Attribute or Value!" );
                return false;
            }

            // Get the resource
            Resource resource = null;
            var resourceAttributeGuid = GetAttributeValue( action, "ResourceAttribute" ).AsGuidOrNull();
            if ( resourceAttributeGuid.HasValue )
            {
                var resourceGuid = action.GetWorkflowAttributeValue( resourceAttributeGuid.Value ).AsGuidOrNull();
                if ( resourceGuid.HasValue && resourceGuid.Value != Guid.Empty )
                {
                    resource = new ResourceService( rockContext ).Get( resourceGuid.Value );
                }
            }
            
            if ( resource == null )
            {
                errorMessages.Add( "Invalid Resource Attribute or Value!" );
                return false;
            }

            // Find the reservation resource
            var reservationResource = reservation.ReservationResources.FirstOrDefault( rr => rr.ResourceId == resource.Id );
            if ( reservationResource == null )
            {
                errorMessages.Add( String.Format( "Resource '{0}' is not assigned to this reservation.", resource.Name ) );
                return false;
            }

            var oldApprovalState = reservation.ApprovalState;
            var changes = new History.HistoryChangeList();
            var resourceName = reservationResource.Resource?.Name ?? resource.Name;

            // Get quantity to remove
            int? quantityToRemove = GetAttributeValue( action, "QuantityToRemove", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
            
            // Determine if we should reduce quantity or remove entirely
            bool shouldRemoveEntirely = true;
            if ( reservationResource.Quantity.HasValue && quantityToRemove.HasValue && quantityToRemove.Value > 0 )
            {
                // Resource has quantity and we want to reduce it
                if ( quantityToRemove.Value >= reservationResource.Quantity.Value )
                {
                    // Remove entirely if quantity to remove is >= current quantity
                    shouldRemoveEntirely = true;
                }
                else
                {
                    // Reduce quantity
                    shouldRemoveEntirely = false;
                }
            }
            else if ( !reservationResource.Quantity.HasValue && quantityToRemove.HasValue && quantityToRemove.Value > 0 )
            {
                // Resource doesn't have quantity but quantity specified - invalid
                errorMessages.Add( "Cannot specify quantity for a non-quantity resource. Resource will be removed entirely." );
                shouldRemoveEntirely = true;
            }

            if ( shouldRemoveEntirely )
            {
                // Remove the resource entirely
                var quantityText = reservationResource.Quantity.HasValue ? reservationResource.Quantity.Value.ToString() : "";
                changes.Add( new History.HistoryChange( History.HistoryVerb.Delete, History.HistoryChangeType.Property, 
                    String.Format( "[Resource] {0} {1}", quantityText, resourceName ) ) );
                reservationResourceService.Delete( reservationResource );
            }
            else
            {
                // Reduce quantity
                var oldQuantity = reservationResource.Quantity.Value;
                var newQuantity = oldQuantity - quantityToRemove.Value;
                reservationResource.Quantity = newQuantity;
                
                changes.Add( new History.HistoryChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, 
                    String.Format( "[Resource] {0} {1} (quantity reduced from {2} to {3})", quantityToRemove.Value, resourceName, oldQuantity, newQuantity ) ) );
            }

            // Update approval state if needed
            // Recalculate approval state based on remaining resources
            reservation = reservationService.UpdateApproval( reservation, reservation.ApprovalState, false );
            if ( oldApprovalState != reservation.ApprovalState )
            {
                History.EvaluateChange(
                    changes,
                    "Approval State",
                    oldApprovalState.ToString(),
                    reservation.ApprovalState.ToString() );
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
