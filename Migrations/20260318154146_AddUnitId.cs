using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace warehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "UQ_Product_Warehouse_Unit_Position",
                table: "Inventories",
                newName: "UQ_Product_Warehouse_Bin");

            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "StockTransferItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_UnitId",
                table: "StockTransferItems",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_Unit",
                table: "StockTransferItems",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_Unit",
                table: "StockTransferItems");

            migrationBuilder.DropIndex(
                name: "IX_StockTransferItems_UnitId",
                table: "StockTransferItems");

            migrationBuilder.DropColumn(
                name: "UnitId",
                table: "StockTransferItems");

            migrationBuilder.RenameIndex(
                name: "UQ_Product_Warehouse_Bin",
                table: "Inventories",
                newName: "UQ_Product_Warehouse_Unit_Position");
        }
    }
}
