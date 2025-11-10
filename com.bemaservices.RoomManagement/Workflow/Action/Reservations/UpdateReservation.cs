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
    /// Updates properties of an existing reservation.
    /// </summary>
    [ActionCategory( "Room Management" )]
    [Description( "Updates properties of an existing reservation. Only provided fields will be updated." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Reservation Update" )]

    [WorkflowAttribute( "Reservation Attribute", "The attribute that contains the reservation to update.", true, "", "", 0, null,
        new string[] { "com.bemaservices.RoomManagement.Field.Types.ReservationFieldType" } )]

    [WorkflowTextOrAttribute( "Name", "Attribute Value", "The new name for the reservation. <span class='tip tip-lava'></span>",
        false, "", "", 1, "Name", new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowAttribute( "Schedule Attribute", "The attribute that contains the new schedule for the reservation.", false, "", "", 2, null,
        new string[] { "Rock.Field.Types.ScheduleFieldType" } )]

    [WorkflowTextOrAttribute( "Note", "Attribute Value", "The new note for the reservation. <span class='tip tip-lava'></span>",
        false, "", "", 3, "Note", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.MemoFieldType" } )]

    [WorkflowTextOrAttribute( "Number Attending", "Attribute Value", "The new number of attendees. <span class='tip tip-lava'></span>",
        false, "", "", 4, "NumberAttending", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    [WorkflowAttribute( "Event Contact Person Attribute", "The attribute that contains the event contact person.", false, "", "", 5, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowTextOrAttribute( "Event Contact Phone", "Attribute Value", "The new event contact phone number. <span class='tip tip-lava'></span>",
        false, "", "", 6, "EventContactPhone", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PhoneNumberFieldType" } )]

    [WorkflowTextOrAttribute( "Event Contact Email", "Attribute Value", "The new event contact email address. <span class='tip tip-lava'></span>",
        false, "", "", 7, "EventContactEmail", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType" } )]

    [WorkflowAttribute( "Administrative Contact Person Attribute", "The attribute that contains the administrative contact person.", false, "", "", 8, null,
        new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowTextOrAttribute( "Administrative Contact Phone", "Attribute Value", "The new administrative contact phone number. <span class='tip tip-lava'></span>",
        false, "", "", 9, "AdministrativeContactPhone", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PhoneNumberFieldType" } )]

    [WorkflowTextOrAttribute( "Administrative Contact Email", "Attribute Value", "The new administrative contact email address. <span class='tip tip-lava'></span>",
        false, "", "", 10, "AdministrativeContactEmail", new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType" } )]

    [WorkflowTextOrAttribute( "Setup Time", "Attribute Value", "The new setup time in minutes. <span class='tip tip-lava'></span>",
        false, "", "", 11, "SetupTime", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    [WorkflowTextOrAttribute( "Cleanup Time", "Attribute Value", "The new cleanup time in minutes. <span class='tip tip-lava'></span>",
        false, "", "", 12, "CleanupTime", new string[] { "Rock.Field.Types.IntegerFieldType" } )]

    [WorkflowAttribute( "Campus Attribute", "The attribute that contains the campus.", false, "", "", 13, null,
        new string[] { "Rock.Field.Types.CampusFieldType" } )]

    [WorkflowAttribute( "Reservation Ministry Attribute", "The attribute that contains the reservation ministry GUID.", false, "", "", 14, null,
        new string[] { "Rock.Field.Types.TextFieldType" } )]

    public class UpdateReservation : ActionComponent
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

            var oldApprovalState = reservation.ApprovalState;
            var changes = new History.HistoryChangeList();
            bool scheduleChanged = false;

            // Update Name
            var nameValue = GetAttributeValue( action, "Name", true ).ResolveMergeFields( mergeFields );
            if ( !nameValue.IsNullOrWhiteSpace() )
            {
                History.EvaluateChange( changes, "Name", reservation.Name, nameValue );
                reservation.Name = nameValue;
            }

            // Update Schedule
            var scheduleAttributeGuid = GetAttributeValue( action, "ScheduleAttribute" ).AsGuidOrNull();
            if ( scheduleAttributeGuid.HasValue )
            {
                var scheduleGuid = action.GetWorkflowAttributeValue( scheduleAttributeGuid.Value ).AsGuidOrNull();
                if ( scheduleGuid.HasValue && scheduleGuid.Value != Guid.Empty )
                {
                    var newSchedule = new ScheduleService( rockContext ).Get( scheduleGuid.Value );
                    if ( newSchedule != null && newSchedule.Id != reservation.ScheduleId )
                    {
                        var oldScheduleText = reservation.GetFriendlyReservationScheduleText();
                        var reservationSchedule = ReservationService.BuildScheduleFromICalContent( newSchedule.iCalendarContent );
                        var scheduleErrorMessage = String.Empty;
                        reservation.Schedule = ReservationService.UpdateScheduleWithMaxEndDate( reservationSchedule, reservation.ReservationType, out scheduleErrorMessage );
                        
                        if ( scheduleErrorMessage.IsNotNullOrWhiteSpace() )
                        {
                            errorMessages.Add( scheduleErrorMessage );
                            if ( reservation.ApprovalState != ReservationApprovalState.Denied )
                            {
                                reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                            }
                        }

                        reservation.ScheduleId = newSchedule.Id;
                        reservation = reservationService.SetFirstLastOccurrenceDateTimes( reservation );
                        scheduleChanged = true;
                        
                        var newScheduleText = reservation.GetFriendlyReservationScheduleText();
                        History.EvaluateChange( changes, "Schedule", oldScheduleText, newScheduleText );
                    }
                }
            }

            // Update Note
            var noteValue = GetAttributeValue( action, "Note", true ).ResolveMergeFields( mergeFields );
            if ( noteValue != null )
            {
                History.EvaluateChange( changes, "Note", reservation.Note, noteValue );
                reservation.Note = noteValue;
            }

            // Update Number Attending
            var numberAttendingValue = GetAttributeValue( action, "NumberAttending", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
            if ( numberAttendingValue.HasValue )
            {
                History.EvaluateChange( changes, "Number Attending", reservation.NumberAttending?.ToString() ?? "", numberAttendingValue.Value.ToString() );
                reservation.NumberAttending = numberAttendingValue.Value;
            }

            // Update Event Contact Person
            var eventContactPersonAttributeGuid = GetAttributeValue( action, "EventContactPersonAttribute" ).AsGuidOrNull();
            if ( eventContactPersonAttributeGuid.HasValue )
            {
                var personGuid = action.GetWorkflowAttributeValue( eventContactPersonAttributeGuid.Value ).AsGuidOrNull();
                if ( personGuid.HasValue && personGuid.Value != Guid.Empty )
                {
                    var person = new PersonService( rockContext ).Get( personGuid.Value );
                    if ( person != null )
                    {
                        var personAlias = person.PrimaryAlias;
                        if ( personAlias != null )
                        {
                            History.EvaluateChange( changes, "Event Contact Person", 
                                reservation.EventContactPersonAlias?.Person?.FullName ?? "", 
                                person.FullName );
                            reservation.EventContactPersonAliasId = personAlias.Id;
                        }
                    }
                }
            }

            // Update Event Contact Phone
            var eventContactPhoneValue = GetAttributeValue( action, "EventContactPhone", true ).ResolveMergeFields( mergeFields );
            if ( eventContactPhoneValue != null )
            {
                History.EvaluateChange( changes, "Event Contact Phone", reservation.EventContactPhone ?? "", eventContactPhoneValue );
                reservation.EventContactPhone = eventContactPhoneValue;
            }

            // Update Event Contact Email
            var eventContactEmailValue = GetAttributeValue( action, "EventContactEmail", true ).ResolveMergeFields( mergeFields );
            if ( eventContactEmailValue != null )
            {
                History.EvaluateChange( changes, "Event Contact Email", reservation.EventContactEmail ?? "", eventContactEmailValue );
                reservation.EventContactEmail = eventContactEmailValue;
            }

            // Update Administrative Contact Person
            var administrativeContactPersonAttributeGuid = GetAttributeValue( action, "AdministrativeContactPersonAttribute" ).AsGuidOrNull();
            if ( administrativeContactPersonAttributeGuid.HasValue )
            {
                var personGuid = action.GetWorkflowAttributeValue( administrativeContactPersonAttributeGuid.Value ).AsGuidOrNull();
                if ( personGuid.HasValue && personGuid.Value != Guid.Empty )
                {
                    var person = new PersonService( rockContext ).Get( personGuid.Value );
                    if ( person != null )
                    {
                        var personAlias = person.PrimaryAlias;
                        if ( personAlias != null )
                        {
                            History.EvaluateChange( changes, "Administrative Contact Person", 
                                reservation.AdministrativeContactPersonAlias?.Person?.FullName ?? "", 
                                person.FullName );
                            reservation.AdministrativeContactPersonAliasId = personAlias.Id;
                        }
                    }
                }
            }

            // Update Administrative Contact Phone
            var administrativeContactPhoneValue = GetAttributeValue( action, "AdministrativeContactPhone", true ).ResolveMergeFields( mergeFields );
            if ( administrativeContactPhoneValue != null )
            {
                History.EvaluateChange( changes, "Administrative Contact Phone", reservation.AdministrativeContactPhone ?? "", administrativeContactPhoneValue );
                reservation.AdministrativeContactPhone = administrativeContactPhoneValue;
            }

            // Update Administrative Contact Email
            var administrativeContactEmailValue = GetAttributeValue( action, "AdministrativeContactEmail", true ).ResolveMergeFields( mergeFields );
            if ( administrativeContactEmailValue != null )
            {
                History.EvaluateChange( changes, "Administrative Contact Email", reservation.AdministrativeContactEmail ?? "", administrativeContactEmailValue );
                reservation.AdministrativeContactEmail = administrativeContactEmailValue;
            }

            // Update Setup Time
            var setupTimeValue = GetAttributeValue( action, "SetupTime", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
            if ( setupTimeValue.HasValue )
            {
                History.EvaluateChange( changes, "Setup Time", reservation.SetupTime?.ToString() ?? "", setupTimeValue.Value.ToString() + " minutes" );
                reservation.SetupTime = setupTimeValue.Value;
            }

            // Update Cleanup Time
            var cleanupTimeValue = GetAttributeValue( action, "CleanupTime", true ).ResolveMergeFields( mergeFields ).AsIntegerOrNull();
            if ( cleanupTimeValue.HasValue )
            {
                History.EvaluateChange( changes, "Cleanup Time", reservation.CleanupTime?.ToString() ?? "", cleanupTimeValue.Value.ToString() + " minutes" );
                reservation.CleanupTime = cleanupTimeValue.Value;
            }

            // Update Campus
            var campusAttributeGuid = GetAttributeValue( action, "CampusAttribute" ).AsGuidOrNull();
            if ( campusAttributeGuid.HasValue )
            {
                var campusGuid = action.GetWorkflowAttributeValue( campusAttributeGuid.Value ).AsGuidOrNull();
                if ( campusGuid.HasValue && campusGuid.Value != Guid.Empty )
                {
                    var campus = new CampusService( rockContext ).Get( campusGuid.Value );
                    if ( campus != null )
                    {
                        History.EvaluateChange( changes, "Campus", reservation.Campus?.Name ?? "", campus.Name );
                        reservation.CampusId = campus.Id;
                    }
                }
                else if ( campusGuid.HasValue && campusGuid.Value == Guid.Empty )
                {
                    // Explicitly clear campus
                    History.EvaluateChange( changes, "Campus", reservation.Campus?.Name ?? "", "" );
                    reservation.CampusId = null;
                }
            }

            // Update Reservation Ministry
            var reservationMinistryAttributeGuid = GetAttributeValue( action, "ReservationMinistryAttribute" ).AsGuidOrNull();
            if ( reservationMinistryAttributeGuid.HasValue )
            {
                var ministryValue = action.GetWorkflowAttributeValue( reservationMinistryAttributeGuid.Value );
                if ( !ministryValue.IsNullOrWhiteSpace() )
                {
                    var ministryGuid = ministryValue.AsGuidOrNull();
                    if ( ministryGuid.HasValue && ministryGuid.Value != Guid.Empty )
                    {
                        var ministry = new ReservationMinistryService( rockContext ).Get( ministryGuid.Value );
                        if ( ministry != null )
                        {
                            History.EvaluateChange( changes, "Reservation Ministry", reservation.ReservationMinistry?.Name ?? "", ministry.Name );
                            reservation.ReservationMinistryId = ministry.Id;
                        }
                    }
                    else
                    {
                        // Try to get by ID if GUID didn't work
                        var ministryId = ministryValue.AsIntegerOrNull();
                        if ( ministryId.HasValue )
                        {
                            var ministry = new ReservationMinistryService( rockContext ).Get( ministryId.Value );
                            if ( ministry != null )
                            {
                                History.EvaluateChange( changes, "Reservation Ministry", reservation.ReservationMinistry?.Name ?? "", ministry.Name );
                                reservation.ReservationMinistryId = ministry.Id;
                            }
                        }
                        else if ( ministryValue.IsNullOrWhiteSpace() || ministryValue == "0" )
                        {
                            // Explicitly clear ministry
                            History.EvaluateChange( changes, "Reservation Ministry", reservation.ReservationMinistry?.Name ?? "", "" );
                            reservation.ReservationMinistryId = null;
                        }
                    }
                }
            }

            // If schedule changed, check for conflicts and update approval state if needed
            if ( scheduleChanged )
            {
                var reservedLocationIds = reservationService.GetReservedLocationIds( reservation, true, false );
                var hasLocationConflicts = reservation.ReservationLocations.Any( rl => reservedLocationIds.Contains( rl.LocationId ) );
                
                if ( hasLocationConflicts )
                {
                    reservation.ApprovalState = ReservationApprovalState.ChangesNeeded;
                }
                else
                {
                    // Recalculate approval state
                    reservation = reservationService.UpdateApproval( reservation, reservation.ApprovalState, false );
                }
            }

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
