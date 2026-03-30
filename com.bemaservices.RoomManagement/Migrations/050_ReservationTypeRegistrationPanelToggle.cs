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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Adds a Reservation Type setting to show/hide the registrations panel.
    /// </summary>
    [MigrationNumber( 50, "1.16.6" )]
    public class ReservationTypeRegistrationPanelToggle : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version.
        /// </summary>
        public override void Up()
        {
            Sql( @"
IF COL_LENGTH('[dbo].[_com_bemaservices_RoomManagement_ReservationType]', 'DisplayReservationRegistrations') IS NULL
BEGIN
    ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]
        ADD [DisplayReservationRegistrations] [bit] NOT NULL CONSTRAINT [DF__com_bemaservices_RoomManagement_ReservationType_DisplayReservationRegistrations] DEFAULT 1;
END
ELSE
BEGIN
    UPDATE [dbo].[_com_bemaservices_RoomManagement_ReservationType]
    SET [DisplayReservationRegistrations] = ISNULL([DisplayReservationRegistrations], 1);
END" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}
