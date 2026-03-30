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
    /// Creates and backfills reservation registration links for reservation API responses.
    /// </summary>
    [MigrationNumber( 49, "1.16.6" )]
    public class LinkedRegistrationApiSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version.
        /// </summary>
        public override void Up()
        {
            Sql( @"
IF OBJECT_ID('[dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]
    (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ReservationId] INT NOT NULL,
        [RegistrationInstanceId] INT NOT NULL,
        [Guid] UNIQUEIDENTIFIER NOT NULL,
        [CreatedDateTime] DATETIME NULL,
        [ModifiedDateTime] DATETIME NULL,
        [CreatedByPersonAliasId] INT NULL,
        [ModifiedByPersonAliasId] INT NULL,
        CONSTRAINT [PK__com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]
        WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink_Reservation]
        FOREIGN KEY([ReservationId]) REFERENCES [dbo].[_com_bemaservices_RoomManagement_Reservation]([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]
        CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink_Reservation];

    ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]
        WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink_RegistrationInstance]
        FOREIGN KEY([RegistrationInstanceId]) REFERENCES [dbo].[RegistrationInstance]([Id]) ON DELETE CASCADE;

    ALTER TABLE [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]
        CHECK CONSTRAINT [FK__com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink_RegistrationInstance];

    CREATE UNIQUE NONCLUSTERED INDEX [IX__com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink_Reservation_Registration]
        ON [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]([ReservationId], [RegistrationInstanceId]);
END

IF OBJECT_ID('[dbo].[_com_bemaservices_RoomManagement_ReservationLinkage]', 'U') IS NOT NULL
BEGIN
    INSERT INTO [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink]
    (
        [ReservationId],
        [RegistrationInstanceId],
        [Guid],
        [CreatedDateTime],
        [ModifiedDateTime],
        [CreatedByPersonAliasId],
        [ModifiedByPersonAliasId]
    )
    SELECT DISTINCT
        rl.[ReservationId],
        ri.[Id] AS [RegistrationInstanceId],
        NEWID(),
        rl.[CreatedDateTime],
        rl.[ModifiedDateTime],
        rl.[CreatedByPersonAliasId],
        rl.[ModifiedByPersonAliasId]
    FROM [dbo].[_com_bemaservices_RoomManagement_ReservationLinkage] rl
    INNER JOIN [dbo].[RegistrationInstance] ri
        ON ri.[EventItemOccurrenceId] = rl.[EventItemOccurrenceId]
    WHERE rl.[ReservationId] IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM [dbo].[_com_bemaservices_RoomManagement_ReservationRegistrationInstanceLink] existingLinks
          WHERE existingLinks.[ReservationId] = rl.[ReservationId]
            AND existingLinks.[RegistrationInstanceId] = ri.[Id]
      );
END
" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {
        }
    }
}
