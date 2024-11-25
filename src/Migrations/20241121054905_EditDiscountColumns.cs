using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ecommerce.Migrations
{
    /// <inheritdoc />
    public partial class EditDiscountColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercentage",
                table: "Products",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercentage",
                table: "ProductCategories",
                type: "decimal(5,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(3,2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DiscountPercentage_Range",
                table: "Products",
                sql: "[DiscountPercentage] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DiscountPercentage_Range1",
                table: "ProductCategories",
                sql: "[DiscountPercentage] BETWEEN 0 AND 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DiscountPercentage_Range",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DiscountPercentage_Range1",
                table: "ProductCategories");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercentage",
                table: "Products",
                type: "decimal(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountPercentage",
                table: "ProductCategories",
                type: "decimal(3,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)");
        }
    }
}
