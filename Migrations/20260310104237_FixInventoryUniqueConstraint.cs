using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace warehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Product_Warehouse",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "UQ_Product_Warehouse_Bin",
                table: "Inventories",
                columns: new[] { "ProductId", "WarehouseId", "StoragePosition" },
                unique: true,
                filter: "[StoragePosition] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UQ_Product_Warehouse_Bin",
                table: "Inventories");

            migrationBuilder.CreateIndex(
                name: "UQ_Product_Warehouse",
                table: "Inventories",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true);
        }
    }
}
