using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CncMes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRichPricingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- 1) Materials tablosu ---
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CostPerCm3 = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    MachinabilityFactor = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    DensityGramPerCm3 = table.Column<decimal>(type: "TEXT", precision: 10, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_Name",
                table: "Materials",
                column: "Name",
                unique: true);

            // 5 hammadde seed (DbInitializer ile aynı değerler — migration ilk açılışta da
            // veri tutarlığı için ekliyor; DbInitializer idempotent, çakışma olmaz).
            migrationBuilder.Sql(
                @"INSERT INTO Materials (Name, CostPerCm3, MachinabilityFactor, DensityGramPerCm3, IsActive) VALUES
                  ('Alüminyum 6061', 1.20, 1.0, 2.70, 1),
                  ('Çelik 1.0037',   2.50, 1.5, 7.85, 1),
                  ('Paslanmaz 304',  4.80, 2.5, 7.90, 1),
                  ('Pirinç CuZn37',  6.20, 1.1, 8.50, 1),
                  ('POM Plastik',    3.40, 0.6, 1.41, 1);");

            // --- 2) PricingSettings: eski alan drop + yeni alanlar ---
            migrationBuilder.DropColumn(
                name: "MaterialCostPerCm3",
                table: "PricingSettings");

            migrationBuilder.AddColumn<decimal>(
                name: "MinutesPerHole",
                table: "PricingSettings",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 1.5m);

            migrationBuilder.AddColumn<decimal>(
                name: "AspectFactorWeight",
                table: "PricingSettings",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0.3m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToleranceFactorRough",
                table: "PricingSettings",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 1.0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToleranceFactorStandard",
                table: "PricingSettings",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 1.3m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToleranceFactorPrecise",
                table: "PricingSettings",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 2.0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToleranceFactorUltraPrecise",
                table: "PricingSettings",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 3.5m);

            migrationBuilder.AddColumn<decimal>(
                name: "SurfaceFinishCostPerCm2Polished",
                table: "PricingSettings",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0.12m);

            migrationBuilder.AddColumn<decimal>(
                name: "SurfaceFinishCostPerCm2Anodized",
                table: "PricingSettings",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 0.20m);

            migrationBuilder.AddColumn<decimal>(
                name: "HeatTreatmentMultiplier",
                table: "PricingSettings",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 1.15m);

            // --- 3) OrderItems: yeni alanlar ---
            migrationBuilder.AddColumn<int>(
                name: "MaterialId",
                table: "OrderItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToleranceGrade",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "Standard");

            migrationBuilder.AddColumn<string>(
                name: "SurfaceFinish",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "AsMilled");

            migrationBuilder.AddColumn<int>(
                name: "HoleCount",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HeatTreated",
                table: "OrderItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManualNotes",
                table: "OrderItems",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AspectRatio",
                table: "OrderItems",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Solidity",
                table: "OrderItems",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ExcessMaterialRatio",
                table: "OrderItems",
                type: "TEXT",
                precision: 10,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClosedMesh",
                table: "OrderItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_MaterialId",
                table: "OrderItems",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Materials_MaterialId",
                table: "OrderItems",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // --- 4) Mevcut OrderItem'lara default Material = 'Çelik 1.0037' ata ---
            migrationBuilder.Sql(
                @"UPDATE OrderItems
                  SET MaterialId = (SELECT Id FROM Materials WHERE Name = 'Çelik 1.0037')
                  WHERE MaterialId IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Materials_MaterialId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_MaterialId",
                table: "OrderItems");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropColumn(name: "AspectFactorWeight", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "HeatTreatmentMultiplier", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "MinutesPerHole", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "SurfaceFinishCostPerCm2Anodized", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "SurfaceFinishCostPerCm2Polished", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "ToleranceFactorPrecise", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "ToleranceFactorRough", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "ToleranceFactorStandard", table: "PricingSettings");
            migrationBuilder.DropColumn(name: "ToleranceFactorUltraPrecise", table: "PricingSettings");

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCostPerCm3",
                table: "PricingSettings",
                type: "TEXT",
                precision: 18,
                scale: 4,
                nullable: false,
                defaultValue: 2.50m);

            migrationBuilder.DropColumn(name: "AspectRatio", table: "OrderItems");
            migrationBuilder.DropColumn(name: "ExcessMaterialRatio", table: "OrderItems");
            migrationBuilder.DropColumn(name: "HeatTreated", table: "OrderItems");
            migrationBuilder.DropColumn(name: "HoleCount", table: "OrderItems");
            migrationBuilder.DropColumn(name: "IsClosedMesh", table: "OrderItems");
            migrationBuilder.DropColumn(name: "ManualNotes", table: "OrderItems");
            migrationBuilder.DropColumn(name: "MaterialId", table: "OrderItems");
            migrationBuilder.DropColumn(name: "Solidity", table: "OrderItems");
            migrationBuilder.DropColumn(name: "SurfaceFinish", table: "OrderItems");
            migrationBuilder.DropColumn(name: "ToleranceGrade", table: "OrderItems");
        }
    }
}
