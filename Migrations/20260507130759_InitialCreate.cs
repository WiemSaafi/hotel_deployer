using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TunisiaStay.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IconClass = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    FullName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    SecurityStamp = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumber = table.Column<string>(type: "TEXT", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "INTEGER", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Classification = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    NumContact = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Address = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ClaimType = table.Column<string>(type: "TEXT", nullable: true),
                    ClaimValue = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderKey = table.Column<string>(type: "TEXT", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    RoleId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    LoginProvider = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    ClientKey = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Numéro = table.Column<int>(type: "INTEGER", nullable: false),
                    Telephone = table.Column<int>(type: "INTEGER", nullable: false),
                    Prénom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Nom = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.ClientKey);
                    table.ForeignKey(
                        name: "FK_Clients_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Avis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Note = table.Column<int>(type: "INTEGER", nullable: false),
                    Commentaire = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HotelId = table.Column<int>(type: "INTEGER", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avis_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Avis_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chambres",
                columns: table => new
                {
                    IdChambre = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Model = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Surface = table.Column<int>(type: "INTEGER", nullable: false),
                    PrixParNuit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Capacite = table.Column<int>(type: "INTEGER", nullable: false),
                    Disponible = table.Column<bool>(type: "INTEGER", nullable: false),
                    Etage = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ImagePath = table.Column<string>(type: "TEXT", nullable: true),
                    HotelId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chambres", x => x.IdChambre);
                    table.ForeignKey(
                        name: "FK_Chambres_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChambreAmenites",
                columns: table => new
                {
                    ChambreId = table.Column<int>(type: "INTEGER", nullable: false),
                    AmeniteId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChambreAmenites", x => new { x.ChambreId, x.AmeniteId });
                    table.ForeignKey(
                        name: "FK_ChambreAmenites_Amenites_AmeniteId",
                        column: x => x.AmeniteId,
                        principalTable: "Amenites",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChambreAmenites_Chambres_ChambreId",
                        column: x => x.ChambreId,
                        principalTable: "Chambres",
                        principalColumn: "IdChambre",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Durée = table.Column<double>(type: "REAL", nullable: false),
                    Prix = table.Column<double>(type: "decimal(18,2)", nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "En attente"),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChambreId = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Chambres_ChambreId",
                        column: x => x.ChambreId,
                        principalTable: "Chambres",
                        principalColumn: "IdChambre",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "ClientKey",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Méthode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Statut = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ReservationId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paiements_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Amenites",
                columns: new[] { "Id", "IconClass", "Nom" },
                values: new object[,]
                {
                    { 1, "fa-wifi", "WiFi Gratuit" },
                    { 2, "fa-swimming-pool", "Piscine" },
                    { 3, "fa-parking", "Parking" },
                    { 4, "fa-snowflake", "Climatisation" },
                    { 5, "fa-utensils", "Restaurant" },
                    { 6, "fa-spa", "Spa & Bien-être" },
                    { 7, "fa-dumbbell", "Salle de sport" },
                    { 8, "fa-water", "Vue mer" },
                    { 9, "fa-coffee", "Petit-déjeuner" },
                    { 10, "fa-shuttle-van", "Navette aéroport" }
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "admin-role-id", "admin-role-id", "Admin", "ADMIN" },
                    { "user-role-id", "user-role-id", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "Hotels",
                columns: new[] { "Id", "Address", "City", "Classification", "CreatedAt", "Description", "Email", "ImagePath", "IsActive", "Latitude", "Longitude", "Name", "NumContact" },
                values: new object[,]
                {
                    { 1, "15 Avenue Habib Bourguiba, Tunis 1000", "Tunis", "5 étoiles", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Niché au cœur de la médina historique de Tunis, cet établissement d'exception marie l'architecture arabo-andalouse authentique avec le luxe contemporain. Classé patrimoine architectural, il offre une expérience unique entre souks et modernité.", "contact@medina-tunis.tn", "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&q=80", true, 36.818899999999999, 10.165800000000001, "Hôtel La Médina Tunis", 71234567 },
                    { 2, "Zone Touristique Sidi Mahrez, Djerba", "Djerba", "5 étoiles", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Resort de luxe face à la mer Méditerranée, sur l'île enchanteresse de Djerba. Plage privée de sable blanc, centre de thalassothérapie primé et gastronomie raffinée vous attendent dans un cadre paradisiaque.", "reservation@hasdrubal-djerba.tn", "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800&q=80", true, 33.807499999999997, 10.8451, "Hasdrubal Thalassa & Spa Djerba", 75609000 },
                    { 3, "Boulevard de la Corniche, Sousse 4000", "Sousse", "5 étoiles", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Véritable palace sur la corniche de Sousse, avec vue imprenable sur la Méditerranée. Ses jardins luxuriants, sa grande piscine et ses suites panoramiques en font la destination idéale pour un séjour de prestige.", "info@marhaba-sousse.tn", "https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?w=800&q=80", true, 35.828800000000001, 10.635, "Marhaba Palace Sousse", 73227700 },
                    { 4, "Avenue des Hôtels, Hammamet 8050", "Hammamet", "4 étoiles", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Hôtel de charme à Hammamet, la Côte d'Azur tunisienne. Entre orangeraies parfumées et plages de sable fin, Le Sindbad propose une expérience authentique dans un cadre verdoyant et apaisé.", "contact@sindbad-hammamet.tn", "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800&q=80", true, 36.3992, 10.5664, "The Sindbad Hammamet", 72280100 }
                });

            migrationBuilder.InsertData(
                table: "Chambres",
                columns: new[] { "IdChambre", "Capacite", "Description", "Disponible", "Etage", "HotelId", "ImagePath", "Model", "PrixParNuit", "Surface" },
                values: new object[,]
                {
                    { 1, 2, "Chambre élégante avec vue sur les toits de la médina, décorée avec des zeliges authentiques et du mobilier en bois de thuya.", true, 2, 1, "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=800&q=80", "Chambre Supérieure", 180m, 30 },
                    { 2, 2, "Suite somptueuse avec salon privatif, baignoire balnéo et terrasse privée avec vue panoramique sur la vieille ville.", true, 4, 1, "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800&q=80", "Suite Deluxe", 350m, 55 },
                    { 3, 4, "L'apogée du luxe tunisien. Deux chambres, grand salon, salle à manger privée et terrasse panoramique avec jacuzzi extérieur.", true, 6, 1, "https://images.unsplash.com/photo-1591088398332-8596b4c8b4c5?w=800&q=80", "Suite Présidentielle", 950m, 130 },
                    { 4, 2, "Chambre lumineuse avec balcon privé et vue directe sur la Méditerranée. Décoration épurée aux tons blancs et bleus.", true, 1, 2, "https://images.unsplash.com/photo-1615460549969-36fa19521a4f?w=800&q=80", "Chambre Vue Mer", 260m, 38 },
                    { 5, 3, "Bungalow exclusif avec accès direct à la plage privée. Terrasse, salon extérieur et piscine à débordement personnelle.", true, 0, 2, "https://images.unsplash.com/photo-1540518614846-7eded433c457?w=800&q=80", "Bungalow Plage", 520m, 70 },
                    { 6, 2, "Chambre deluxe avec balcon et vue sur la piscine et la mer. Équipements haut de gamme et literie premium.", true, 3, 3, "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800&q=80", "Chambre Deluxe", 210m, 35 },
                    { 7, 2, "Suite junior avec vue mer panoramique, coin salon séparé et salle de bain en marbre avec douche à l'italienne.", true, 5, 3, "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&q=80", "Suite Junior", 320m, 50 },
                    { 8, 2, "Chambre confortable et bien équipée avec vue sur le jardin. Idéale pour un séjour ensoleillé à Hammamet.", true, 1, 4, "https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=800&q=80", "Chambre Standard", 140m, 26 },
                    { 9, 4, "Chambre spacieuse pour familles avec deux lits doubles, espace jeux et balcon donnant sur l'orangeraie.", true, 2, 4, "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800&q=80", "Chambre Familiale", 240m, 48 }
                });

            migrationBuilder.InsertData(
                table: "ChambreAmenites",
                columns: new[] { "AmeniteId", "ChambreId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 4, 1 },
                    { 9, 1 },
                    { 1, 2 },
                    { 4, 2 },
                    { 6, 2 },
                    { 9, 2 },
                    { 1, 3 },
                    { 2, 3 },
                    { 4, 3 },
                    { 5, 3 },
                    { 6, 3 },
                    { 1, 4 },
                    { 4, 4 },
                    { 8, 4 },
                    { 1, 5 },
                    { 2, 5 },
                    { 6, 5 },
                    { 8, 5 },
                    { 1, 6 },
                    { 4, 6 },
                    { 9, 6 },
                    { 1, 7 },
                    { 4, 7 },
                    { 6, 7 },
                    { 8, 7 },
                    { 1, 8 },
                    { 3, 8 },
                    { 4, 8 },
                    { 1, 9 },
                    { 3, 9 },
                    { 4, 9 },
                    { 9, 9 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Avis_ApplicationUserId",
                table: "Avis",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Avis_HotelId",
                table: "Avis",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChambreAmenites_AmeniteId",
                table: "ChambreAmenites",
                column: "AmeniteId");

            migrationBuilder.CreateIndex(
                name: "IX_Chambres_HotelId",
                table: "Chambres",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ApplicationUserId",
                table: "Clients",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_Email",
                table: "Hotels",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_ReservationId",
                table: "Paiements",
                column: "ReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ChambreId",
                table: "Reservations",
                column: "ChambreId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ClientId",
                table: "Reservations",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Avis");

            migrationBuilder.DropTable(
                name: "ChambreAmenites");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Amenites");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Chambres");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
