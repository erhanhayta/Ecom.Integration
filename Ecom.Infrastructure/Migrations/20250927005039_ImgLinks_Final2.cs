using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecom.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImgLinks_Final2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImageLinks_Products_ProductId",
                table: "ProductImageLinks");

            migrationBuilder.DropIndex(
                name: "IX_ProductImageLinks_ProductId_ProductVariantId_ProductImageId",
                table: "ProductImageLinks");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImageLinks_ProductId_ProductImageId_ProductVariantId",
                table: "ProductImageLinks",
                columns: new[] { "ProductId", "ProductImageId", "ProductVariantId" },
                unique: true,
                filter: "[ProductVariantId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductImageLinks_ProductId_ProductImageId_ProductVariantId",
                table: "ProductImageLinks");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImageLinks_ProductId_ProductVariantId_ProductImageId",
                table: "ProductImageLinks",
                columns: new[] { "ProductId", "ProductVariantId", "ProductImageId" },
                unique: true,
                filter: "[ProductVariantId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImageLinks_Products_ProductId",
                table: "ProductImageLinks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
