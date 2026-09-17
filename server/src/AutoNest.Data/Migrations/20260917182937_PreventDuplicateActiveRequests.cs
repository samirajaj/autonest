using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoNest.Data.Migrations
{
    /// <inheritdoc />
    public partial class PreventDuplicateActiveRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                WITH RankedRequests AS
                (
                    SELECT [Id],
                           ROW_NUMBER() OVER
                           (
                               PARTITION BY [CustomerId], [CarId]
                               ORDER BY CASE WHEN [State] = 2 THEN 0 ELSE 1 END, [RequestDate] DESC, [Id] DESC
                           ) AS [RowNumber]
                    FROM [dbo].[Requests]
                    WHERE [State] IN (1, 2)
                )
                UPDATE [Requests]
                SET [State] = 3
                FROM [dbo].[Requests] AS [Requests]
                INNER JOIN [RankedRequests] ON [RankedRequests].[Id] = [Requests].[Id]
                WHERE [RankedRequests].[RowNumber] > 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Requests_CustomerId",
                schema: "dbo",
                table: "Requests");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CustomerId_CarId",
                schema: "dbo",
                table: "Requests",
                columns: new[] { "CustomerId", "CarId" },
                unique: true,
                filter: "[State] IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_CustomerId_CarId",
                schema: "dbo",
                table: "Requests");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_CustomerId",
                schema: "dbo",
                table: "Requests",
                column: "CustomerId");
        }
    }
}
