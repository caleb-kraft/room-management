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
using Rock.Plugin;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 50, "1.16.6" )]
    public class MinistryDefinedType : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Add DefinedValueId column to ReservationMinistry table
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
                ADD [DefinedValueId] [int] NULL
            " );

            // Add foreign key constraint
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]  
                WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_DefinedValueId] 
                FOREIGN KEY([DefinedValueId])
                REFERENCES [dbo].[DefinedValue] ([Id])
            " );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] 
                CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_DefinedValueId]
            " );

            // Add MinistryDefinedTypeId column to ReservationType table
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]
                ADD [MinistryDefinedTypeId] [int] NULL
            " );

            // Add foreign key constraint
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]  
                WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_MinistryDefinedTypeId] 
                FOREIGN KEY([MinistryDefinedTypeId])
                REFERENCES [dbo].[DefinedType] ([Id])
            " );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] 
                CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_MinistryDefinedTypeId]
            " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType] 
                DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationType_MinistryDefinedTypeId]
            " );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationType]
                DROP COLUMN [MinistryDefinedTypeId]
            " );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry] 
                DROP CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationMinistry_DefinedValueId]
            " );

            Sql( @"
                ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationMinistry]
                DROP COLUMN [DefinedValueId]
            " );
        }
    }
}
