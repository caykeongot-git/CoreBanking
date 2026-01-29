using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreBanking.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIsKycVerified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KycInfo",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncome",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KycInfo",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MonthlyIncome",
                table: "Customers");
        }
    }
}
