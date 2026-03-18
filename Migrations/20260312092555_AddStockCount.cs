using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace warehouseManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddStockCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "AdjustmentReasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdjustmentReasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StockCountSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CountNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCountSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockCountSessions_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockCountSessions_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockCountSessions_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockCountItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockCountSessionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    StoragePosition = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SystemQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Difference = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ReasonId = table.Column<int>(type: "int", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockCountItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockCountItems_AdjustmentReasons_ReasonId",
                        column: x => x.ReasonId,
                        principalTable: "AdjustmentReasons",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StockCountItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StockCountItems_StockCountSessions_StockCountSessionId",
                        column: x => x.StockCountSessionId,
                        principalTable: "StockCountSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Product_Warehouse_Unit_Position",
                table: "Inventories",
                columns: new[] { "ProductId", "WarehouseId", "StoragePosition" },
                unique: true,
                filter: "[StoragePosition] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountItems_ProductId",
                table: "StockCountItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountItems_ReasonId",
                table: "StockCountItems",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountItems_StockCountSessionId_ProductId_StoragePosition",
                table: "StockCountItems",
                columns: new[] { "StockCountSessionId", "ProductId", "StoragePosition" },
                unique: true,
                filter: "[StoragePosition] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountSessions_ApprovedBy",
                table: "StockCountSessions",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountSessions_CountNo",
                table: "StockCountSessions",
                column: "CountNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockCountSessions_CreatedBy",
                table: "StockCountSessions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StockCountSessions_WarehouseId",
                table: "StockCountSessions",
                column: "WarehouseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StockCountItems");

            migrationBuilder.DropTable(
                name: "AdjustmentReasons");

            migrationBuilder.DropTable(
                name: "StockCountSessions");
        }
    }
}
