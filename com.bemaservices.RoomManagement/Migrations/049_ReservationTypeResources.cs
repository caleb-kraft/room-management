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
using System.Data.Entity;
using System.Linq;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 49, "1.16.6" )]
    public class ReservationTypeResources : RoomManagementMigration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ReservationTypeId] [int] NOT NULL,
	[ResourceId] [int] NOT NULL,
	[Quantity] [int] NULL,
	[CreatedDateTime] [datetime] NULL,
	[ModifiedDateTime] [datetime] NULL,
	[CreatedByPersonAliasId] [int] NULL,
	[ModifiedByPersonAliasId] [int] NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[ForeignKey] [nvarchar](100) NULL,
	[ForeignGuid] [uniqueidentifier] NULL,
	[ForeignId] [int] NULL,
 CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationTypeResource] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_CreatedByPersonAliasId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
REFERENCES [dbo].[PersonAlias] ([Id])

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_ModifiedByPersonAliasId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_ReservationTypeId] FOREIGN KEY([ReservationTypeId])
REFERENCES [dbo].[_com_bemaservices_RoomManagement_ReservationType] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_ReservationTypeId]

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource]  WITH CHECK ADD  CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_ResourceId] FOREIGN KEY([ResourceId])
REFERENCES [dbo].[_com_bemaservices_RoomManagement_Resource] ([Id])
ON DELETE CASCADE

ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource] CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationTypeResource_ResourceId]
" );
            RoomManagementMigrationHelper.UpdateEntityTypeByGuid( "com.bemaservices.RoomManagement.Model.ReservationTypeResource", "Reservation Type Resource", "com.bemaservices.RoomManagement.Model.ReservationTypeResource, com.bemaservices.RoomManagement, Version=1.2.2.0, Culture=neutral, PublicKeyToken=null", true, true, "B8E4B3B0-B543-48B6-93BE-604D4F368560" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
            Sql( @"DROP TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationTypeResource]" );
        }
    }
}
