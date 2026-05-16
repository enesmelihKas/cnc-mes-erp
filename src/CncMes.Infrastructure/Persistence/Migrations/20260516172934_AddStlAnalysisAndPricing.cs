using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CncMes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStlAnalysisAndPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AnalyzedAt",
                table: "OrderItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BoundingBoxXCm",
                table: "OrderItems",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BoundingBoxYCm",
                table: "OrderItems",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BoundingBoxZCm",
                table: "OrderItems",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedMinutes",
                table: "OrderItems",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedUnitPrice",
                table: "OrderItems",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StlFileName",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SurfaceAreaCm2",
                table: "OrderItems",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TriangleCount",
                table: "OrderItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "VolumeCm3",
                table: "OrderItems",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PricingSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    MaterialCostPerCm3 = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    LaborCostPerMinute = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    MachineHourlyRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    BaseSetupCost = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    MinutesPerCm3 = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    MinutesPerThousandTriangles = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    ComplexityMultiplier = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingSettings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PricingSettings");

            migrationBuilder.DropColumn(
                name: "AnalyzedAt",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "BoundingBoxXCm",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "BoundingBoxYCm",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "BoundingBoxZCm",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "EstimatedMinutes",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "EstimatedUnitPrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "StlFileName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "SurfaceAreaCm2",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "TriangleCount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VolumeCm3",
                table: "OrderItems");
        }
    }
}
