using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CncMes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCostTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ElectricityCostPerKwh",
                table: "PricingSettings",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MachinePowerKw",
                table: "PricingSettings",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToolWearCostPerMinute",
                table: "PricingSettings",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CostEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AmountTL = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Detail = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CostEntries_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CostEntries_JobId",
                table: "CostEntries",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_CostEntries_Type",
                table: "CostEntries",
                column: "Type");

            // Mevcut DB'de PricingSettings singleton'ı zaten var; DbInitializer onu
            // yeniden seed etmediği için yeni 3 alan 0 kalırdı (elektrik/takım hep 0).
            // İdempotent backfill: yalnızca 0 olanlara makul defaultlar yaz.
            migrationBuilder.Sql(
                "UPDATE PricingSettings SET ElectricityCostPerKwh = 3.5 WHERE Id = 1 AND ElectricityCostPerKwh = 0;");
            migrationBuilder.Sql(
                "UPDATE PricingSettings SET MachinePowerKw = 7.5 WHERE Id = 1 AND MachinePowerKw = 0;");
            migrationBuilder.Sql(
                "UPDATE PricingSettings SET ToolWearCostPerMinute = 1.2 WHERE Id = 1 AND ToolWearCostPerMinute = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CostEntries");

            migrationBuilder.DropColumn(
                name: "ElectricityCostPerKwh",
                table: "PricingSettings");

            migrationBuilder.DropColumn(
                name: "MachinePowerKw",
                table: "PricingSettings");

            migrationBuilder.DropColumn(
                name: "ToolWearCostPerMinute",
                table: "PricingSettings");
        }
    }
}
