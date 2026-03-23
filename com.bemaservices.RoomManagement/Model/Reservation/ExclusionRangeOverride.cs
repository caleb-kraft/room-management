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
using System.Runtime.Serialization;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// Represents field overrides for a specific exclusion date range in a recurring reservation.
    /// </summary>
    [DataContract]
    public class ExclusionRangeOverride
    {
        /// <summary>
        /// Gets or sets the start date of the exclusion range.
        /// </summary>
        /// <value>The start date.</value>
        [DataMember]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the exclusion range.
        /// </summary>
        /// <value>The end date.</value>
        [DataMember]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the event contact person alias identifier override.
        /// </summary>
        /// <value>The event contact person alias identifier.</value>
        [DataMember]
        public int? EventContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the event contact phone override.
        /// </summary>
        /// <value>The event contact phone.</value>
        [DataMember]
        public string EventContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the event contact email override.
        /// </summary>
        /// <value>The event contact email.</value>
        [DataMember]
        public string EventContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact person alias identifier override.
        /// </summary>
        /// <value>The administrative contact person alias identifier.</value>
        [DataMember]
        public int? AdministrativeContactPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact phone override.
        /// </summary>
        /// <value>The administrative contact phone.</value>
        [DataMember]
        public string AdministrativeContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the administrative contact email override.
        /// </summary>
        /// <value>The administrative contact email.</value>
        [DataMember]
        public string AdministrativeContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the location overrides for this exclusion range.
        /// Key is the location ID, value indicates if it should be included (true) or excluded (false).
        /// If null or empty, uses default locations from the reservation.
        /// </summary>
        /// <value>The location overrides.</value>
        [DataMember]
        public Dictionary<int, bool> LocationOverrides { get; set; }

        /// <summary>
        /// Gets or sets the resource overrides for this exclusion range.
        /// Key is the resource ID, value is the quantity override (null means use default).
        /// </summary>
        /// <value>The resource overrides.</value>
        [DataMember]
        public Dictionary<int, int?> ResourceOverrides { get; set; }

        /// <summary>
        /// Gets or sets the setup time override (in minutes).
        /// </summary>
        /// <value>The setup time override.</value>
        [DataMember]
        public int? SetupTimeOverride { get; set; }

        /// <summary>
        /// Gets or sets the cleanup time override (in minutes).
        /// </summary>
        /// <value>The cleanup time override.</value>
        [DataMember]
        public int? CleanupTimeOverride { get; set; }

        /// <summary>
        /// Gets or sets the number attending override.
        /// </summary>
        /// <value>The number attending override.</value>
        [DataMember]
        public int? NumberAttendingOverride { get; set; }

        /// <summary>
        /// Gets or sets the note override.
        /// </summary>
        /// <value>The note override.</value>
        [DataMember]
        public string NoteOverride { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier override.
        /// </summary>
        /// <value>The campus identifier override.</value>
        [DataMember]
        public int? CampusIdOverride { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusionRangeOverride"/> class.
        /// </summary>
        public ExclusionRangeOverride()
        {
            LocationOverrides = new Dictionary<int, bool>();
            ResourceOverrides = new Dictionary<int, int?>();
        }

        /// <summary>
        /// Determines whether the specified date falls within this exclusion range.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns><c>true</c> if the date falls within the range; otherwise, <c>false</c>.</returns>
        public bool ContainsDate( DateTime date )
        {
            var dateOnly = date.Date;
            return dateOnly >= StartDate.Date && dateOnly <= EndDate.Date;
        }
    }
}

