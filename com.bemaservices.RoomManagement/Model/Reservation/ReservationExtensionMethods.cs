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
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using Rock;
using Rock.Data;
using Rock.Model;
using com.bemaservices.RoomManagement.Utility.RockInternalMethods;
using Ical.Net;
using Ical.Net.CalendarComponents;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Class ReservationExtensionMethods.
    /// </summary>
    public static partial class ReservationExtensionMethods
    {
        /// <summary>
        /// Gets the reservation summaries.
        /// </summary>
        /// <param name="qry">The qry.</param>
        /// <param name="filterStartDateTime">The filter start date time.</param>
        /// <param name="filterEndDateTime">The filter end date time.</param>
        /// <param name="roundToDay">if set to <c>true</c> [round to day].</param>
        /// <param name="includeAttributes">if set to <c>true</c> [include attributes].</param>
        /// <param name="maxOccurrences">The maximum occurrences.</param>
        /// <param name="filterTimeBy">The filter time by.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        public static List<Model.ReservationSummary> GetReservationSummaries( this IQueryable<Reservation> qry, DateTime? filterStartDateTime, DateTime? filterEndDateTime, bool roundToDay = false, bool includeAttributes = false, int? maxOccurrences = null, FilterTimeBy filterTimeBy = FilterTimeBy.Reservation )
        {
            var reservationSummaryList = new List<Model.ReservationSummary>();

            if ( qry == null )
            {
                return reservationSummaryList;
            }

            if ( filterStartDateTime == null )
            {
                filterStartDateTime = DateTime.Now;
            }

            if ( filterEndDateTime == null )
            {
                filterEndDateTime = DateTime.Now.AddMonths( 1 );
            }

            if ( filterStartDateTime < DateTime.MinValue.AddYears( 1 ) )
            {
                filterStartDateTime = DateTime.MinValue.AddYears( 1 );
            }
            if ( filterEndDateTime > DateTime.MaxValue.AddYears( -1 ) )
            {
                filterEndDateTime = DateTime.MaxValue.AddYears( -1 );
            }

            var qryStartDateTime = filterStartDateTime.Value.AddMonths( -1 );
            var qryEndDateTime = filterEndDateTime.Value.AddMonths( 1 );
            if ( roundToDay )
            {
                filterEndDateTime = filterEndDateTime.Value.AddDays( 1 ).AddMilliseconds( -1 );
            }

            // OPTIMIZATION: Use AsNoTracking() to avoid entity tracking overhead since we're just reading data
            // Also filter more aggressively using FirstOccurrenceStartDateTime/LastOccurrenceEndDateTime
            var reservations = qry
                .AsNoTracking()
                .Where( r => r.FirstOccurrenceStartDateTime == null || r.FirstOccurrenceStartDateTime <= filterEndDateTime )
                .Where( r => r.LastOccurrenceEndDateTime == null || r.LastOccurrenceEndDateTime >= filterStartDateTime )
                .Where( r => r.Schedule.iCalendarContent.Contains( "RRULE" ) || r.Schedule.iCalendarContent.Contains( "RDATE" ) ||
                        (
                            r.Schedule.EffectiveStartDate >= qryStartDateTime &&
                            r.Schedule.EffectiveEndDate <= qryEndDateTime )
                        )
                        .ToList();

            // OPTIMIZATION: Generate occurrences lazily and filter early to avoid processing unnecessary reservations
            // CRITICAL: For conflict checking, we must generate ALL occurrences that could overlap with the filter range
            // The filter range represents the new reservation's schedule range, so we must check ALL occurrences in that range
            var reservationsWithDates = new List<ReservationDate>();
            foreach ( var reservation in reservations )
            {
                // CRITICAL: Always ensure we cover the FULL filter range to catch all conflicts for the new reservation's entire schedule
                // Start with the filter range as the base (this is what we MUST check)
                var occurrenceStartDate = filterStartDateTime.Value;
                var occurrenceEndDate = filterEndDateTime.Value;
                
                // OPTIMIZATION: We can expand to include buffer (qryStartDateTime/qryEndDateTime) to catch edge cases,
                // but only if the reservation's FirstOccurrenceStartDateTime/LastOccurrenceEndDateTime don't restrict us
                
                // Expand start date to include buffer if reservation starts before or at filter start
                if ( !reservation.FirstOccurrenceStartDateTime.HasValue || 
                     reservation.FirstOccurrenceStartDateTime.Value <= filterStartDateTime.Value )
                {
                    // Reservation starts before/at filter start - safe to use buffer start
                    occurrenceStartDate = qryStartDateTime;
                }
                // else: Reservation starts after filter start - keep filterStartDateTime to ensure we cover new reservation's occurrences
                
                // Expand end date to include buffer if reservation ends after or at filter end
                if ( !reservation.LastOccurrenceEndDateTime.HasValue || 
                     reservation.LastOccurrenceEndDateTime.Value >= filterEndDateTime.Value )
                {
                    // Reservation ends after/at filter end - safe to use buffer end
                    occurrenceEndDate = qryEndDateTime;
                }
                // else: Reservation ends before filter end - keep filterEndDateTime to ensure we cover new reservation's occurrences
                
                // Generate occurrences for this reservation
                // Note: We're guaranteed to cover at least filterStartDateTime to filterEndDateTime,
                // which ensures we'll check all occurrences in the new reservation's schedule
                if ( occurrenceStartDate <= occurrenceEndDate )
                {
                    var reservationDateTimes = reservation.GetReservationTimes( occurrenceStartDate, occurrenceEndDate );
                    if ( reservationDateTimes.Any() )
                    {
                        reservationsWithDates.Add( new ReservationDate
                        {
                            Reservation = reservation,
                            ReservationDateTimes = reservationDateTimes
                        } );
                    }
                }
            }

            foreach ( var reservationWithDates in reservationsWithDates )
            {
                // OPTIMIZATION: Early exit if we've reached maxOccurrences
                if ( maxOccurrences != null && reservationSummaryList.Count >= maxOccurrences )
                {
                    break;
                }

                var reservation = reservationWithDates.Reservation;

                if ( includeAttributes )
                {
                    reservation.LoadAttributes();
                }

                foreach ( var reservationDateTime in reservationWithDates.ReservationDateTimes )
                {
                    // OPTIMIZATION: Early exit if we've reached maxOccurrences
                    if ( maxOccurrences != null && reservationSummaryList.Count >= maxOccurrences )
                    {
                        break;
                    }

                    var reservationStartDateTime = reservationDateTime.StartDateTime.AddMinutes( -reservation.SetupTime ?? 0 );
                    var reservationEndDateTime = reservationDateTime.EndDateTime.AddMinutes( reservation.CleanupTime ?? 0 );

                    // Check if this is a recurring reservation
                    var calEvent = InetCalendarHelper.CreateCalendarEvent( reservation.Schedule?.iCalendarContent ?? "" );
                    var isRecurring = calEvent != null && ( ( calEvent.RecurrenceRules?.Any() == true ) || ( calEvent.RecurrenceDates?.Any() == true ) );

                    // Check if this is an exception occurrence (linked to original via ForeignKey)
                    var isExceptionOccurrence = !string.IsNullOrWhiteSpace( reservation.ForeignKey ) && 
                                                reservation.ForeignKey.StartsWith( "OriginalReservation_" );
                    int? originalReservationId = null;
                    if ( isExceptionOccurrence )
                    {
                        var foreignKeyParts = reservation.ForeignKey.Split( '_' );
                        if ( foreignKeyParts.Length > 1 && int.TryParse( foreignKeyParts[1], out int originalId ) )
                        {
                            originalReservationId = originalId;
                        }
                    }

                    var validReservationTime = false;
                    if (
                        ( filterTimeBy == FilterTimeBy.Reservation || filterTimeBy == FilterTimeBy.Both ) &&
                        ( ( reservationStartDateTime >= filterStartDateTime ) || ( reservationEndDateTime >= filterStartDateTime ) ) &&
                        ( ( reservationStartDateTime < filterEndDateTime ) || ( reservationEndDateTime < filterEndDateTime ) )
                       )
                    {
                        validReservationTime = true;
                    }

                    var validDoorLockTime = false;
                    List<ReservationDoorLockTime> reservationDoorLockTimes = new List<ReservationDoorLockTime>();
                    var orderedReservationDoorLockSchedules = reservation.ReservationDoorLockSchedules.OrderBy( rdls => rdls.StartTimeOffset ).ToList();
                    foreach ( var reservationDoorLockSchedule in orderedReservationDoorLockSchedules )
                    {
                        var reservationDoorLockTime = new ReservationDoorLockTime(
                                reservationDateTime.StartDateTime.AddMinutes( reservationDoorLockSchedule.StartTimeOffset ),
                                reservationDateTime.StartDateTime.AddMinutes( reservationDoorLockSchedule.EndTimeOffset ),
                                reservationDoorLockSchedule.Note
                                );
                        reservationDoorLockTimes.Add( reservationDoorLockTime );

                        if (
                            ( filterTimeBy == FilterTimeBy.DoorLock || filterTimeBy == FilterTimeBy.Both ) &&
                            ( ( reservationDoorLockTime.StartDateTime >= filterStartDateTime ) || ( reservationDoorLockTime.EndDateTime >= filterStartDateTime ) ) &&
                            ( ( reservationDoorLockTime.StartDateTime < filterEndDateTime ) || ( reservationDoorLockTime.EndDateTime < filterEndDateTime ) )
                           )
                        {
                            validDoorLockTime = true;
                        }
                    }

                    // Removed 9/17/2024: If no custom ones are set, we want to leave it up to the HVAC provider whether to
                    // use the default times or ignore the reservation.
                    //if ( !reservationDoorLockTimes.Any() )
                    //{
                    //    reservationDoorLockTimes.Add( new ReservationDoorLockTime( reservationStartDateTime, reservationEndDateTime,"Default" ) );
                    //}

                    if ( validReservationTime || validDoorLockTime )
                    {
                        // Check for exclusion range overrides for this occurrence date
                        var exclusionOverride = reservation.GetExclusionRangeOverrideForDate( reservationDateTime.StartDateTime );
                        
                        // Apply overrides if they exist
                        var effectiveSetupTime = exclusionOverride?.SetupTimeOverride ?? reservation.SetupTime ?? 0;
                        var effectiveCleanupTime = exclusionOverride?.CleanupTimeOverride ?? reservation.CleanupTime ?? 0;
                        var effectiveNumberAttending = exclusionOverride?.NumberAttendingOverride ?? reservation.NumberAttending;
                        var effectiveNote = exclusionOverride?.NoteOverride ?? reservation.Note;
                        PersonAlias effectiveEventContactPersonAlias = reservation.EventContactPersonAlias;
                        if ( exclusionOverride?.EventContactPersonAliasId.HasValue == true )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                effectiveEventContactPersonAlias = new PersonAliasService( rockContext ).Get( exclusionOverride.EventContactPersonAliasId.Value );
                            }
                        }
                        var effectiveEventContactEmail = exclusionOverride?.EventContactEmail ?? reservation.EventContactEmail;
                        var effectiveEventContactPhone = exclusionOverride?.EventContactPhone ?? reservation.EventContactPhone;
                        PersonAlias effectiveAdministrativeContactPersonAlias = reservation.AdministrativeContactPersonAlias;
                        if ( exclusionOverride?.AdministrativeContactPersonAliasId.HasValue == true )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                effectiveAdministrativeContactPersonAlias = new PersonAliasService( rockContext ).Get( exclusionOverride.AdministrativeContactPersonAliasId.Value );
                            }
                        }
                        var effectiveAdministrativeContactEmail = exclusionOverride?.AdministrativeContactEmail ?? reservation.AdministrativeContactEmail;
                        var effectiveAdministrativeContactPhone = exclusionOverride?.AdministrativeContactPhone ?? reservation.AdministrativeContactPhone;

                        // Apply location and resource overrides
                        var effectiveLocations = reservation.ReservationLocations.ToList();
                        var effectiveResources = reservation.ReservationResources.ToList();
                        
                        if ( exclusionOverride != null )
                        {
                            // Filter locations based on overrides
                            if ( exclusionOverride.LocationOverrides.Any() )
                            {
                                effectiveLocations = effectiveLocations.Where( l => 
                                    exclusionOverride.LocationOverrides.ContainsKey( l.LocationId ) && 
                                    exclusionOverride.LocationOverrides[l.LocationId] 
                                ).ToList();
                            }

                            // Override resource quantities
                            if ( exclusionOverride.ResourceOverrides.Any() )
                            {
                                foreach ( var resource in effectiveResources )
                                {
                                    if ( exclusionOverride.ResourceOverrides.ContainsKey( resource.ResourceId ) )
                                    {
                                        var overrideQuantity = exclusionOverride.ResourceOverrides[resource.ResourceId];
                                        if ( overrideQuantity.HasValue )
                                        {
                                            resource.Quantity = overrideQuantity.Value;
                                        }
                                    }
                                }
                            }
                        }

                        // Recalculate reservation times with overridden setup/cleanup times
                        var effectiveReservationStartDateTime = reservationDateTime.StartDateTime.AddMinutes( -effectiveSetupTime );
                        var effectiveReservationEndDateTime = reservationDateTime.EndDateTime.AddMinutes( effectiveCleanupTime );

                        var reservationSummary = new Model.ReservationSummary
                        {
                            Id = reservation.Id,
                            ReservationType = reservation.ReservationType,
                            ApprovalState = reservation.ApprovalState,
                            ReservationName = reservation.Name,
                            ReservationLocations = effectiveLocations,
                            ReservationResources = effectiveResources,
                            UnassignedReservationResources = reservation.UnassignedReservationResources.ToList(),
                            ReservationDoorLockTimes = reservationDoorLockTimes,
                            EventStartDateTime = reservationDateTime.StartDateTime,
                            EventEndDateTime = reservationDateTime.EndDateTime,
                            ReservationStartDateTime = effectiveReservationStartDateTime,
                            ReservationEndDateTime = effectiveReservationEndDateTime,
                            EventDateTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime ),
                            EventTimeDescription = GetFriendlyScheduleDescription( reservationDateTime.StartDateTime, reservationDateTime.EndDateTime, false ),
                            ReservationDateTimeDescription = GetFriendlyScheduleDescription( effectiveReservationStartDateTime, effectiveReservationEndDateTime ),
                            ReservationTimeDescription = GetFriendlyScheduleDescription( effectiveReservationStartDateTime, effectiveReservationEndDateTime, false ),
                            ReservationMinistry = reservation.ReservationMinistry,
                            EventContactPersonAlias = effectiveEventContactPersonAlias,
                            EventContactEmail = effectiveEventContactEmail,
                            EventContactPhoneNumber = effectiveEventContactPhone,
                            AdministrativeContactPersonAlias = effectiveAdministrativeContactPersonAlias,
                            AdministrativeContactEmail = effectiveAdministrativeContactEmail,
                            AdministrativeContactPhoneNumber = effectiveAdministrativeContactPhone,
                            SetupPhotoId = reservation.SetupPhotoId,
                            SetupPhotoGuid = reservation.SetupPhoto?.Guid,
                            Note = effectiveNote,
                            RequesterAlias = reservation.RequesterAlias,
                            NumberAttending = effectiveNumberAttending,
                            ModifiedDateTime = reservation.ModifiedDateTime,
                            ScheduleId = reservation.ScheduleId,
                            IsRecurring = isRecurring,
                            IsExceptionOccurrence = isExceptionOccurrence,
                            OriginalReservationId = originalReservationId
                        };

                        if ( includeAttributes )
                        {
                            reservationSummary.Attributes = reservation.Attributes;
                            reservationSummary.AttributeValues = reservation.AttributeValues;

                            foreach ( var reservationLocation in reservationSummary.ReservationLocations )
                            {
                                reservationLocation.LoadAttributes();
                            }

                            foreach ( var reservationResource in reservationSummary.ReservationResources )
                            {
                                reservationResource.LoadAttributes();
                            }
                        }

                        reservationSummaryList.Add( reservationSummary );

                        // Exit if the number of instance of this specific event has exceeded the occurrence limit.
                        if ( maxOccurrences != null && reservationSummaryList.Count >= maxOccurrences )
                        {
                            break;
                        }
                    }
                }
            }

            // Pass 2: Sort all of the event occurrences by date, and then apply the occurrence limit.
            if ( maxOccurrences != null )
            {
                reservationSummaryList = reservationSummaryList
                    .OrderBy( x => x.ReservationStartDateTime )
                    .Take( maxOccurrences.Value )
                    .ToList();

            }

            return reservationSummaryList;
        }

        /// <summary>
        /// Valids the existing reservations.
        /// </summary>
        /// <param name="reservations">The reservations.</param>
        /// <param name="reservationId">The reservation identifier.</param>
        /// <param name="arePotentialConflictsReturned">if set to <c>true</c> [are potential conflicts returned].</param>
        /// <returns>IQueryable&lt;Reservation&gt;.</returns>
        public static IQueryable<Reservation> ValidExistingReservations( this IQueryable<Reservation> reservations, int? reservationId = null, bool arePotentialConflictsReturned = false )
        {
            var validReservations = reservations.Where( r => r.ApprovalState != ReservationApprovalState.Denied
                                                                     && r.ApprovalState != ReservationApprovalState.Draft
                                                                     && r.ApprovalState != ReservationApprovalState.Cancelled
                                                                     && (
                                                                         ( arePotentialConflictsReturned == false && ( !r.ReservationType.IsReservationBookedOnApproval || r.ApprovalState == ReservationApprovalState.Approved ) ) ||
                                                                         ( arePotentialConflictsReturned == true && r.ReservationType.IsReservationBookedOnApproval && r.ApprovalState != ReservationApprovalState.Approved )
                                                                         )

                                                         );
            if ( reservationId != null )
            {
                validReservations = validReservations.Where( r => r.Id != reservationId );
            }

            // Make sure communication wasn't just recently approved
            return validReservations;
        }

        /// <summary>
        /// Wheres the conflicts exist.
        /// </summary>
        /// <param name="existingReservationSummaries">The existing reservation summaries.</param>
        /// <param name="newReservationSummaries">The new reservation summaries.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        public static List<ReservationSummary> WhereConflictsExist( this List<ReservationSummary> existingReservationSummaries, List<ReservationSummary> newReservationSummaries )
        {
            var conflictingSummaries = existingReservationSummaries.Where( existingReservationSummary => existingReservationSummary.MatchingSummaries( newReservationSummaries ).Any() ).ToList();
            return conflictingSummaries;
        }

        /// <summary>
        /// Matchings the summaries.
        /// </summary>
        /// <param name="sourceReservationSummary">The source reservation summary.</param>
        /// <param name="potentialSummaryMatches">The potential summary matches.</param>
        /// <returns>List&lt;ReservationSummary&gt;.</returns>
        public static List<ReservationSummary> MatchingSummaries( this ReservationSummary sourceReservationSummary, List<ReservationSummary> potentialSummaryMatches )
        {
            var matchingSummaries = potentialSummaryMatches.Where( potentialSummaryMatch =>
                 ( sourceReservationSummary.ReservationStartDateTime >= potentialSummaryMatch.ReservationStartDateTime || sourceReservationSummary.ReservationEndDateTime > potentialSummaryMatch.ReservationStartDateTime ) &&
                 ( sourceReservationSummary.ReservationStartDateTime < potentialSummaryMatch.ReservationEndDateTime || sourceReservationSummary.ReservationEndDateTime < potentialSummaryMatch.ReservationEndDateTime )
                 ).ToList();
            return matchingSummaries;
        }

        /// <summary>
        /// Reserveds the resource quantity.
        /// </summary>
        /// <param name="reservationSummaries">The reservation summaries.</param>
        /// <param name="resourceId">The resource identifier.</param>
        /// <returns>System.Int32.</returns>
        public static int ReservedResourceQuantity( this List<ReservationSummary> reservationSummaries, int resourceId )
        {
            var reservedQuantity = reservationSummaries
                .DistinctBy( reservationSummary => reservationSummary.Id )
                .Sum( reservationSummary =>
                    reservationSummary.ReservationResources
                    .Where( rr => rr.Quantity.HasValue && rr.ApprovalState != ReservationResourceApprovalState.Denied && rr.ResourceId == resourceId )
                    .Sum( rr => rr.Quantity.Value )
                    );
            return reservedQuantity;
        }

        /// <summary>
        /// Gets the friendly schedule description.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="showDate">if set to <c>true</c> [show date].</param>
        /// <returns>System.String.</returns>
        public static string GetFriendlyScheduleDescription( DateTime startDateTime, DateTime endDateTime, bool showDate = true )
        {
            if ( startDateTime.Date == endDateTime.Date )
            {
                if ( showDate )
                {
                    return String.Format( "{0} {1} - {2}", startDateTime.ToString( "MM/dd" ), startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "hh:mmt" ).ToLower() );
                }
                else
                {
                    return String.Format( "{0} - {1}", startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "hh:mmt" ).ToLower() );
                }
            }
            else
            {
                return String.Format( "{0} {1} - {2} {3}", startDateTime.ToString( "MM/dd/yy" ), startDateTime.ToString( "hh:mmt" ).ToLower(), endDateTime.ToString( "MM/dd/yy" ), endDateTime.ToString( "hh:mmt" ).ToLower() );
            }
        }

        /// <summary>
        /// Clones this Reservation object to a new Reservation object
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="deepCopy">if set to <c>true</c> a deep copy is made. If false, only the basic entity properties are copied.</param>
        /// <returns>Reservation.</returns>
        public static Reservation Clone( this Reservation source, bool deepCopy )
        {
            if ( deepCopy )
            {
                return source.Clone() as Reservation;
            }
            else
            {
                var target = new Reservation();
                target.CopyPropertiesFrom( source );
                return target;
            }
        }

        /// <summary>
        /// Copies the properties from another Reservation object to this Reservation object
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this Reservation target, Reservation source )
        {
            target.Id = source.Id;
            target.Name = source.Name;

            target.Schedule = source.Schedule;
            target.ScheduleId = source.ScheduleId;

            target.CampusId = source.CampusId;
            target.EventItemOccurrenceId = source.EventItemOccurrenceId;
            target.ReservationMinistryId = source.ReservationMinistryId;

            //target.ApprovalState = source.ApprovalState;
            target.RequesterAliasId = source.RequesterAliasId;
            //target.ApproverAliasId = source.ApproverAliasId;
            target.SetupTime = source.SetupTime;
            target.CleanupTime = source.CleanupTime;
            target.NumberAttending = source.NumberAttending;
            target.Note = source.Note;
            target.SetupPhotoId = source.SetupPhotoId;
            target.EventContactPersonAlias = source.EventContactPersonAlias;
            target.EventContactPersonAliasId = source.EventContactPersonAliasId;
            target.EventContactPhone = source.EventContactPhone;
            target.EventContactEmail = source.EventContactEmail;
            target.AdministrativeContactPersonAlias = source.AdministrativeContactPersonAlias;
            target.AdministrativeContactPersonAliasId = source.AdministrativeContactPersonAliasId;
            target.AdministrativeContactPhone = source.AdministrativeContactPhone;
            target.AdministrativeContactEmail = source.AdministrativeContactEmail;

            target.ReservationLocations = source.ReservationLocations;
            target.ReservationResources = source.ReservationResources;

            target.CreatedDateTime = source.CreatedDateTime;
            target.ModifiedDateTime = source.ModifiedDateTime;
            target.CreatedByPersonAliasId = source.CreatedByPersonAliasId;
            target.ModifiedByPersonAliasId = source.ModifiedByPersonAliasId;
            target.Guid = source.Guid;
            target.ForeignId = source.ForeignId;
            target.ForeignGuid = source.ForeignGuid;
            target.ForeignKey = source.ForeignKey;
        }

        /// <summary>
        /// Copies the properties from.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="source">The source.</param>
        public static void CopyPropertiesFrom( this ReservationService.ReservationSummary target, Model.ReservationSummary source )
        {
            target.Id = source.Id;
            target.ReservationType = source.ReservationType;
            target.ApprovalState = source.ApprovalState;
            target.ReservationName = source.ReservationName;
            target.ReservationMinistry = source.ReservationMinistry;
            target.SetupPhotoId = source.SetupPhotoId;
            target.ScheduleId = source.ScheduleId;
            target.NumberAttending = source.NumberAttending;
            target.Note = source.Note;

            target.EventDateTimeDescription = source.EventDateTimeDescription;
            target.EventTimeDescription = source.EventTimeDescription;
            target.ReservationDateTimeDescription = source.ReservationDateTimeDescription;
            target.ReservationTimeDescription = source.ReservationTimeDescription;

            target.ReservationStartDateTime = source.ReservationStartDateTime;
            target.ReservationEndDateTime = source.ReservationEndDateTime;
            target.EventStartDateTime = source.EventStartDateTime;
            target.EventEndDateTime = source.EventEndDateTime;

            target.ReservationLocations = source.ReservationLocations;
            target.ReservationResources = source.ReservationResources;
            target.UnassignedReservationResources = source.UnassignedReservationResources;
            target.ReservationDoorLockTimes = source.ReservationDoorLockTimes;

            target.EventContactPersonAlias = source.EventContactPersonAlias;
            target.EventContactPhoneNumber = source.EventContactPhoneNumber;
            target.EventContactEmail = source.EventContactEmail;
            target.AdministrativeContactPersonAlias = source.AdministrativeContactPersonAlias;
            target.AdministrativeContactPhoneNumber = source.AdministrativeContactPhoneNumber;
            target.AdministrativeContactEmail = source.AdministrativeContactEmail;
            target.RequesterAlias = source.RequesterAlias;


            target.ModifiedDateTime = source.ModifiedDateTime;
            target.Attributes = source.Attributes;
            target.AttributeValues = source.AttributeValues;
        }

        /// <summary>
        /// Enum FilterTimeBy
        /// </summary>
        public enum FilterTimeBy
        {
            /// <summary>
            /// The reservation
            /// </summary>
            Reservation = 0,

            /// <summary>
            /// The door lock
            /// </summary>
            DoorLock = 1,

            /// <summary>
            /// The both
            /// </summary>
            Both = 2
        }

    }
}
