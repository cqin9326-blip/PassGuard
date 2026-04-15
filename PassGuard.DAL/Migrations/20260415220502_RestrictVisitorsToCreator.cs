using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassGuard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RestrictVisitorsToCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visitors_FullName_Phone",
                table: "Visitors");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Visitors",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_CreatedByUserId_FullName_Phone",
                table: "Visitors",
                columns: new[] { "CreatedByUserId", "FullName", "Phone" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visitors_CreatedByUserId_FullName_Phone",
                table: "Visitors");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Visitors");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_FullName_Phone",
                table: "Visitors",
                columns: new[] { "FullName", "Phone" },
                unique: true);
        }
    }
}
