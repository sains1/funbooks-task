using System;

using Microsoft.EntityFrameworkCore.Migrations;

using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShippingService.Migrations
{
    /// <inheritdoc />
    public partial class AddsShippableProductsAndCustomers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shipping");

            migrationBuilder.CreateTable(
                name: "CustomerAddresses",
                schema: "shipping",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressLine1 = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AddressLine2 = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PostCode = table.Column<string>(type: "text", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddresses", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "ShippableProducts",
                schema: "shipping",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Sku = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    WeightKg = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippableProducts", x => x.ProductId);
                });

            migrationBuilder.InsertData(
                schema: "shipping",
                table: "CustomerAddresses",
                columns: new[] { "CustomerId", "AddressLine1", "AddressLine2", "PostCode" },
                values: new object[,]
                {
                    { 12345, "123 Main St", "Apt 101", "SW1 1AA" },
                    { 56789, "789 Oak St", "Unit 303", "AB0 2XY" },
                });

            migrationBuilder.InsertData(
                schema: "shipping",
                table: "ShippableProducts",
                columns: new[] { "ProductId", "ProductName", "Sku", "WeightKg" },
                values: new object[] { new Guid("6831ee62-b099-44e7-b3e2-d2cd045cc2f5"), "The Girl on the Train", "TGOTT1", 0.50m });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerAddresses",
                schema: "shipping");

            migrationBuilder.DropTable(
                name: "ShippableProducts",
                schema: "shipping");
        }
    }
}