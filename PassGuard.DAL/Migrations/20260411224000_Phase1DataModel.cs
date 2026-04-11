using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PassGuard.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Phase1DataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GateCheckIns_VisitPassId",
                table: "GateCheckIns");

            migrationBuilder.DropColumn(
                name: "VisitorName",
                table: "VisitPasses");

            migrationBuilder.DropColumn(
                name: "VisitorPhone",
                table: "VisitPasses");

            migrationBuilder.DropColumn(
                name: "OwnerName",
                table: "Homes");

            migrationBuilder.AddColumn<string>(
                name: "CodeHash",
                table: "VisitPasses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "VisitPasses",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VisitorId",
                table: "VisitPasses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Homes",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SecurityUserId",
                table: "GateCheckIns",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    VisitorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitors", x => x.VisitorId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitPasses_VisitorId",
                table: "VisitPasses",
                column: "VisitorId");

            migrationBuilder.CreateIndex(
                name: "IX_GateCheckIns_VisitPassId",
                table: "GateCheckIns",
                column: "VisitPassId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VisitPasses_Visitors_VisitorId",
                table: "VisitPasses",
                column: "VisitorId",
                principalTable: "Visitors",
                principalColumn: "VisitorId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VisitPasses_Visitors_VisitorId",
                table: "VisitPasses");

            migrationBuilder.DropTable(
                name: "Visitors");

            migrationBuilder.DropIndex(
                name: "IX_VisitPasses_VisitorId",
                table: "VisitPasses");

            migrationBuilder.DropIndex(
                name: "IX_GateCheckIns_VisitPassId",
                table: "GateCheckIns");

            migrationBuilder.DropColumn(
                name: "CodeHash",
                table: "VisitPasses");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "VisitPasses");

            migrationBuilder.DropColumn(
                name: "VisitorId",
                table: "VisitPasses");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Homes");

            migrationBuilder.DropColumn(
                name: "SecurityUserId",
                table: "GateCheckIns");

            migrationBuilder.AddColumn<string>(
                name: "VisitorName",
                table: "VisitPasses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VisitorPhone",
                table: "VisitPasses",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OwnerName",
                table: "Homes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GateCheckIns_VisitPassId",
                table: "GateCheckIns",
                column: "VisitPassId");
        }
    }
}
