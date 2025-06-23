using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddParcelExtraFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParcelCategory",
                table: "Parcels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderAddress",
                table: "Parcels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Parcels",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParcelCategory",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "SenderAddress",
                table: "Parcels");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Parcels");
        }
    }
}
