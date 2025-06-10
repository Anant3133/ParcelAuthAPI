using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParcelAuthAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentLocationToParcel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentLocation",
                table: "Parcels",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentLocation",
                table: "Parcels");
        }
    }
}
