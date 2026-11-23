using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApp.Migrations
{
    /// <inheritdoc />
    public partial class MigrateExistingOwners : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @OwnerMap TABLE (
                    OldOwnerId nvarchar(256) NOT NULL,
                    NewUserId nvarchar(256) NOT NULL,
                    Provider nvarchar(64) NOT NULL,
                    ProviderKey nvarchar(256) NOT NULL,
                    NeedsInsert bit NOT NULL
                );

                WITH DistinctLegacyOwners AS (
                    SELECT DISTINCT
                        l.OwnerId AS OldOwnerId,
                        CASE
                            WHEN l.OwnerId LIKE 'microsoft:%' THEN 'Microsoft'
                            WHEN l.OwnerId LIKE 'google:%' THEN 'Google'
                        END AS Provider,
                        SUBSTRING(l.OwnerId, CHARINDEX(':', l.OwnerId) + 1, LEN(l.OwnerId)) AS ProviderKey
                    FROM todo.TodoLists l
                    WHERE l.OwnerId IS NOT NULL
                      AND (l.OwnerId LIKE 'microsoft:%' OR l.OwnerId LIKE 'google:%')
                )
                INSERT INTO @OwnerMap (OldOwnerId, NewUserId, Provider, ProviderKey, NeedsInsert)
                SELECT
                    d.OldOwnerId,
                    COALESCE(ul.UserId, CONVERT(nvarchar(256), NEWID())),
                    d.Provider,
                    d.ProviderKey,
                    CASE WHEN ul.UserId IS NULL THEN 1 ELSE 0 END
                FROM DistinctLegacyOwners d
                LEFT JOIN todo.UserLogins ul
                    ON ul.Provider = d.Provider AND ul.ProviderKey = d.ProviderKey;

                INSERT INTO todo.Users (Id, CreatedAt)
                SELECT NewUserId, SYSUTCDATETIME()
                FROM @OwnerMap
                WHERE NeedsInsert = 1;

                INSERT INTO todo.UserLogins (UserId, Provider, ProviderKey, CreatedAt)
                SELECT NewUserId, Provider, ProviderKey, SYSUTCDATETIME()
                FROM @OwnerMap
                WHERE NeedsInsert = 1;

                UPDATE l
                SET l.OwnerId = m.NewUserId
                FROM todo.TodoLists l
                INNER JOIN @OwnerMap m ON l.OwnerId = m.OldOwnerId;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
