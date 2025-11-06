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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Model;

namespace com.bemaservices.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Type Resource - links a reservation type to resources that should be automatically added
    /// </summary>
    [Table( "_com_bemaservices_RoomManagement_ReservationTypeResource" )]
    [DataContract]
    public class ReservationTypeResource : Rock.Data.Model<ReservationTypeResource>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation type identifier.
        /// </summary>
        /// <value>The reservation type identifier.</value>
        [Required]
        [DataMember]
        public int ReservationTypeId { get; set; }

        /// <summary>
        /// Gets or sets the resource identifier.
        /// </summary>
        /// <value>The resource identifier.</value>
        [Required]
        [DataMember]
        public int ResourceId { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>The quantity.</value>
        [DataMember]
        public int? Quantity { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the resource.
        /// </summary>
        /// <value>The resource.</value>
        [DataMember]
        public virtual Resource Resource { get; set; }

        /// <summary>
        /// Gets or sets the type of the reservation.
        /// </summary>
        /// <value>The type of the reservation.</value>
        [DataMember]
        public virtual ReservationType ReservationType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// The EF configuration for the ReservationTypeResource
    /// </summary>
    public partial class ReservationTypeResourceConfiguration : EntityTypeConfiguration<ReservationTypeResource>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationTypeResourceConfiguration" /> class.
        /// </summary>
        public ReservationTypeResourceConfiguration()
        {
            this.HasRequired( p => p.Resource ).WithMany().HasForeignKey( p => p.ResourceId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.ReservationType ).WithMany( p => p.ReservationTypeResources ).HasForeignKey( p => p.ReservationTypeId ).WillCascadeOnDelete( true );

            // IMPORTANT!!
            this.HasEntitySetName( "ReservationTypeResource" );
        }
    }

    #endregion
}
