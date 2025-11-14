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
    /// Migration for adding ExclusionRangeOverrides field to Reservation table.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 049, "1.16.7" )]
    public class ExclusionRangeOverrides : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Add ExclusionRangeOverrides column to Reservation table
            Sql( @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[_com_bemaservices_RoomManagement_Reservation]') AND name = 'ExclusionRangeOverrides')
                BEGIN
                    ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]
                    ADD [ExclusionRangeOverrides] NVARCHAR(MAX) NULL
                END
            " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            // Remove ExclusionRangeOverrides column from Reservation table
            Sql( @"
                IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[_com_bemaservices_RoomManagement_Reservation]') AND name = 'ExclusionRangeOverrides')
                BEGIN
                    ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_Reservation]
                    DROP COLUMN [ExclusionRangeOverrides]
                END
            " );
        }
    }
}

