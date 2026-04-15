using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassGuard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Phase10ValidationHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Homes_EstateId",
                table: "Homes");

            migrationBuilder.CreateIndex(
                name: "IX_VisitPasses_CodeHash",
                table: "VisitPasses",
                column: "CodeHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_FullName_Phone",
                table: "Visitors",
                columns: new[] { "FullName", "Phone" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Homes_EstateId_Address",
                table: "Homes",
                columns: new[] { "EstateId", "Address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Homes_OwnerUserId",
                table: "Homes",
                column: "OwnerUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estates_EstateName",
                table: "Estates",
                column: "EstateName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_VisitPasses_CodeHash",
                table: "VisitPasses");

            migrationBuilder.DropIndex(
                name: "IX_Visitors_FullName_Phone",
                table: "Visitors");

            migrationBuilder.DropIndex(
                name: "IX_Homes_EstateId_Address",
                table: "Homes");

            migrationBuilder.DropIndex(
                name: "IX_Homes_OwnerUserId",
                table: "Homes");

            migrationBuilder.DropIndex(
                name: "IX_Estates_EstateName",
                table: "Estates");

            migrationBuilder.CreateIndex(
                name: "IX_Homes_EstateId",
                table: "Homes",
                column: "EstateId");
        }
    }
}
