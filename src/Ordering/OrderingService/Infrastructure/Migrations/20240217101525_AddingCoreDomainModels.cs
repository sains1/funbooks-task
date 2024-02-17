using System;

using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderingService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddingCoreDomainModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "ordering");

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "ordering",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                schema: "ordering",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                schema: "ordering",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseOrderNumber = table.Column<int>(type: "integer", nullable: false),
                    TotalCost = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => new { x.CustomerId, x.PurchaseOrderNumber });
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "ordering",
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineItems",
                schema: "ordering",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    PurchaseOrderNumber = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineItems", x => new { x.CustomerId, x.PurchaseOrderNumber, x.ProductId });
                    table.ForeignKey(
                        name: "FK_LineItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "ordering",
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LineItems_PurchaseOrders_CustomerId_PurchaseOrderNumber",
                        columns: x => new { x.CustomerId, x.PurchaseOrderNumber },
                        principalSchema: "ordering",
                        principalTable: "PurchaseOrders",
                        principalColumns: new[] { "CustomerId", "PurchaseOrderNumber" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "ordering",
                table: "Customers",
                column: "CustomerId",
                values: new object[]
                {
                    12345,
                    56789
                });

            migrationBuilder.InsertData(
                schema: "ordering",
                table: "Products",
                columns: new[] { "ProductId", "Price", "ProductName", "Type" },
                values: new object[,]
                {
                    { new Guid("1d217f91-bef1-4eb6-ada8-d9d36739c03e"), 19.99m, "Comprehensive First Aid Training", 1 },
                    { new Guid("3ea5f11d-c4ee-4f08-bdde-82559c7bd0af"), 29.99m, "Book Club", 2 },
                    { new Guid("6831ee62-b099-44e7-b3e2-d2cd045cc2f5"), 9.99m, "The Girl on the Train", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_LineItems_ProductId",
                schema: "ordering",
                table: "LineItems",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LineItems",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "Products",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "PurchaseOrders",
                schema: "ordering");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "ordering");
        }
    }
}