using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TunisiaStay.Migrations
{
    /// <inheritdoc />
    public partial class AddOffres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Offres",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titre = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Pourcentage = table.Column<int>(type: "INTEGER", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HotelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ChambreIdChambre = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offres_Chambres_ChambreIdChambre",
                        column: x => x.ChambreIdChambre,
                        principalTable: "Chambres",
                        principalColumn: "IdChambre");
                    table.ForeignKey(
                        name: "FK_Offres_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offres_ChambreIdChambre",
                table: "Offres",
                column: "ChambreIdChambre");

            migrationBuilder.CreateIndex(
                name: "IX_Offres_HotelId",
                table: "Offres",
                column: "HotelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Offres");
        }
    }
}
