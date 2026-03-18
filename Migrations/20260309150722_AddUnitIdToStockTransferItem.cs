using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace warehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitIdToStockTransferItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Chỉ thêm UnitId vào StockTransferItems, default = 1 (base unit)
            migrationBuilder.AddColumn<int>(
                name: "UnitId",
                table: "StockTransferItems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_UnitId",
                table: "StockTransferItems",
                column: "UnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_Unit",
                table: "StockTransferItems",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
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
        }
    }
}