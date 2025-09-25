using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MP_Tables_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketplaceAttributeMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Firm = table.Column<int>(type: "int", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    LocalField = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceAttributeMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Firm = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceProducts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceProductAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketplaceProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    AttributeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ValueId = table.Column<int>(type: "int", nullable: true),
                    ValueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Required = table.Column<bool>(type: "bit", nullable: false),
                    Varianter = table.Column<bool>(type: "bit", nullable: false),
                    AllowCustom = table.Column<bool>(type: "bit", nullable: false),
                    Slicer = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceProductAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceProductAttributes_MarketplaceProducts_MarketplaceProductId",
                        column: x => x.MarketplaceProductId,
                        principalTable: "MarketplaceProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MarketplaceVariantAttributes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketplaceProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttributeId = table.Column<int>(type: "int", nullable: false),
                    ValueId = table.Column<int>(type: "int", nullable: true),
                    ValueName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketplaceVariantAttributes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceVariantAttributes_MarketplaceProducts_MarketplaceProductId",
                        column: x => x.MarketplaceProductId,
                        principalTable: "MarketplaceProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceAttributeMappings_Firm_AttributeId",
                table: "MarketplaceAttributeMappings",
                columns: new[] { "Firm", "AttributeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceProductAttributes_MarketplaceProductId_AttributeId",
                table: "MarketplaceProductAttributes",
                columns: new[] { "MarketplaceProductId", "AttributeId" });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceProducts_CategoryId",
                table: "MarketplaceProducts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceProducts_ProductId_ShopId_Firm",
                table: "MarketplaceProducts",
                columns: new[] { "ProductId", "ShopId", "Firm" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceVariantAttributes_MarketplaceProductId_ProductVariantId_AttributeId",
                table: "MarketplaceVariantAttributes",
                columns: new[] { "MarketplaceProductId", "ProductVariantId", "AttributeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MarketplaceAttributeMappings");

            migrationBuilder.DropTable(
                name: "MarketplaceProductAttributes");

            migrationBuilder.DropTable(
                name: "MarketplaceVariantAttributes");

            migrationBuilder.DropTable(
                name: "MarketplaceProducts");
        }
    }
}
