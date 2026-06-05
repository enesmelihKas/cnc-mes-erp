using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CncMes.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaterialStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LowStockThreshold",
                table: "Materials",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StockKg",
                table: "Materials",
                type: "TEXT",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);

            // Mevcut seed hammaddelerine makul stok defaultları ata (Phase 5).
            // İdempotent: sadece StockKg=0 olanlar etkilenir, manuel düzenlemeyi ezmez.
            migrationBuilder.Sql("UPDATE Materials SET StockKg = 150, LowStockThreshold = 25 WHERE Name = 'Alüminyum 6061' AND StockKg = 0;");
            migrationBuilder.Sql("UPDATE Materials SET StockKg = 200, LowStockThreshold = 40 WHERE Name = 'Çelik 1.0037'   AND StockKg = 0;");
            migrationBuilder.Sql("UPDATE Materials SET StockKg =  80, LowStockThreshold = 20 WHERE Name = 'Paslanmaz 304'  AND StockKg = 0;");
            migrationBuilder.Sql("UPDATE Materials SET StockKg =  50, LowStockThreshold = 15 WHERE Name = 'Pirinç CuZn37'  AND StockKg = 0;");
            migrationBuilder.Sql("UPDATE Materials SET StockKg =  30, LowStockThreshold = 10 WHERE Name = 'POM Plastik'    AND StockKg = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "StockKg",
                table: "Materials");
        }
    }
}
