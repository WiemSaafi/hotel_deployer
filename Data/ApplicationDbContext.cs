using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TunisiaStay.Models;

namespace TunisiaStay.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Hotel> Hotels => Set<Hotel>();
        public DbSet<Chambre> Chambres => Set<Chambre>();
        public DbSet<Offre> Offres => Set<Offre>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<Paiement> Paiements => Set<Paiement>();
        public DbSet<Amenite> Amenites => Set<Amenite>();
        public DbSet<ChambreAmenite> ChambreAmenites => Set<ChambreAmenite>();
        public DbSet<Avis> Avis => Set<Avis>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ChambreAmenite>().HasKey(ca => new { ca.ChambreId, ca.AmeniteId });
            builder.Entity<ChambreAmenite>().HasOne(ca => ca.Chambre).WithMany(c => c.ChambreAmenites).HasForeignKey(ca => ca.ChambreId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ChambreAmenite>().HasOne(ca => ca.Amenite).WithMany(a => a.ChambreAmenites).HasForeignKey(ca => ca.AmeniteId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Hotel>(e => {
                e.HasKey(h => h.Id);
                e.Property(h => h.Name).IsRequired().HasMaxLength(200);
                e.Property(h => h.Classification).IsRequired().HasMaxLength(50);
                e.Property(h => h.Email).IsRequired().HasMaxLength(200);
                e.HasIndex(h => h.Email).IsUnique();
            });

            builder.Entity<Chambre>(e => {
                e.HasKey(c => c.IdChambre);
                e.Property(c => c.Model).IsRequired().HasMaxLength(100);
                e.Property(c => c.PrixParNuit).HasColumnType("decimal(18,2)").IsRequired();
                e.HasOne(c => c.Hotel).WithMany(h => h.Chambres).HasForeignKey(c => c.HotelId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Client>(e => {
                e.HasKey(c => c.ClientKey);
                e.Property(c => c.Email).IsRequired().HasMaxLength(200);
                e.HasIndex(c => c.Email).IsUnique();
                e.HasOne(c => c.ApplicationUser).WithOne(u => u.Client).HasForeignKey<Client>(c => c.ApplicationUserId).OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Reservation>(e => {
                e.HasKey(r => r.Id);
                e.Property(r => r.Statut).HasMaxLength(50).HasDefaultValue("En attente");
                e.HasOne(r => r.Chambre).WithMany(c => c.Reservations).HasForeignKey(r => r.ChambreId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Client).WithMany(c => c.Reservations).HasForeignKey(r => r.ClientId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Paiement>(e => {
                e.HasKey(p => p.Id);
                e.Property(p => p.Montant).HasColumnType("decimal(18,2)").IsRequired();
                e.HasOne(p => p.Reservation).WithOne(r => r.Paiement).HasForeignKey<Paiement>(p => p.ReservationId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Avis>(e => {
                e.HasKey(a => a.Id);
                e.HasOne(a => a.Hotel).WithMany(h => h.Avis).HasForeignKey(a => a.HotelId).OnDelete(DeleteBehavior.Cascade);
            });

            SeedData(builder);
        }

        private static void SeedData(ModelBuilder builder)
        {
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().HasData(
                new Microsoft.AspNetCore.Identity.IdentityRole { Id = "admin-role-id", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "admin-role-id" },
                new Microsoft.AspNetCore.Identity.IdentityRole { Id = "user-role-id",  Name = "User",  NormalizedName = "USER",  ConcurrencyStamp = "user-role-id" },
                new Microsoft.AspNetCore.Identity.IdentityRole { Id = "hotelier-role-id", Name = "Hotelier", NormalizedName = "HOTELIER", ConcurrencyStamp = "hotelier-role-id" }
            );

            builder.Entity<Amenite>().HasData(
                new Amenite { Id = 1, Nom = "WiFi Gratuit",   IconClass = "fa-wifi" },
                new Amenite { Id = 2, Nom = "Piscine",        IconClass = "fa-swimming-pool" },
                new Amenite { Id = 3, Nom = "Parking",        IconClass = "fa-parking" },
                new Amenite { Id = 4, Nom = "Climatisation",  IconClass = "fa-snowflake" },
                new Amenite { Id = 5, Nom = "Restaurant",     IconClass = "fa-utensils" },
                new Amenite { Id = 6, Nom = "Spa & Bien-être",IconClass = "fa-spa" },
                new Amenite { Id = 7, Nom = "Salle de sport", IconClass = "fa-dumbbell" },
                new Amenite { Id = 8, Nom = "Vue mer",        IconClass = "fa-water" },
                new Amenite { Id = 9, Nom = "Petit-déjeuner", IconClass = "fa-coffee" },
                new Amenite { Id = 10,Nom = "Navette aéroport",IconClass = "fa-shuttle-van" }
            );

            // ── Real Tunisian hotel images (Unsplash - free)
            builder.Entity<Hotel>().HasData(
                new Hotel {
                    Id = 1, Name = "Hôtel La Médina Tunis",
                    Classification = "5 étoiles", Email = "contact@medina-tunis.tn",
                    NumContact = 71234567, City = "Tunis",
                    Address = "15 Avenue Habib Bourguiba, Tunis 1000",
                    Description = "Niché au cœur de la médina historique de Tunis, cet établissement d'exception marie l'architecture arabo-andalouse authentique avec le luxe contemporain. Classé patrimoine architectural, il offre une expérience unique entre souks et modernité.",
                    Latitude = 36.8189, Longitude = 10.1658, IsActive = true,
                    ImagePath = "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&q=80",
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Hotel {
                    Id = 2, Name = "Hasdrubal Thalassa & Spa Djerba",
                    Classification = "5 étoiles", Email = "reservation@hasdrubal-djerba.tn",
                    NumContact = 75609000, City = "Djerba",
                    Address = "Zone Touristique Sidi Mahrez, Djerba",
                    Description = "Resort de luxe face à la mer Méditerranée, sur l'île enchanteresse de Djerba. Plage privée de sable blanc, centre de thalassothérapie primé et gastronomie raffinée vous attendent dans un cadre paradisiaque.",
                    Latitude = 33.8075, Longitude = 10.8451, IsActive = true,
                    ImagePath = "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?w=800&q=80",
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Hotel {
                    Id = 3, Name = "Marhaba Palace Sousse",
                    Classification = "5 étoiles", Email = "info@marhaba-sousse.tn",
                    NumContact = 73227700, City = "Sousse",
                    Address = "Boulevard de la Corniche, Sousse 4000",
                    Description = "Véritable palace sur la corniche de Sousse, avec vue imprenable sur la Méditerranée. Ses jardins luxuriants, sa grande piscine et ses suites panoramiques en font la destination idéale pour un séjour de prestige.",
                    Latitude = 35.8288, Longitude = 10.6350, IsActive = true,
                    ImagePath = "https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?w=800&q=80",
                    CreatedAt = new DateTime(2024, 1, 1)
                },
                new Hotel {
                    Id = 4, Name = "The Sindbad Hammamet",
                    Classification = "4 étoiles", Email = "contact@sindbad-hammamet.tn",
                    NumContact = 72280100, City = "Hammamet",
                    Address = "Avenue des Hôtels, Hammamet 8050",
                    Description = "Hôtel de charme à Hammamet, la Côte d'Azur tunisienne. Entre orangeraies parfumées et plages de sable fin, Le Sindbad propose une expérience authentique dans un cadre verdoyant et apaisé.",
                    Latitude = 36.3992, Longitude = 10.5664, IsActive = true,
                    ImagePath = "https://images.unsplash.com/photo-1566073771259-6a8506099945?w=800&q=80",
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // ── Real room images (Unsplash - free)
            builder.Entity<Chambre>().HasData(
                // Hôtel Médina Tunis
                new Chambre { IdChambre = 1, Model = "Chambre Supérieure", Surface = 30, PrixParNuit = 180, Capacite = 2, Disponible = true, Etage = 2, HotelId = 1,
                    Description = "Chambre élégante avec vue sur les toits de la médina, décorée avec des zeliges authentiques et du mobilier en bois de thuya.",
                    ImagePath = "https://images.unsplash.com/photo-1631049307264-da0ec9d70304?w=800&q=80" },
                new Chambre { IdChambre = 2, Model = "Suite Deluxe", Surface = 55, PrixParNuit = 350, Capacite = 2, Disponible = true, Etage = 4, HotelId = 1,
                    Description = "Suite somptueuse avec salon privatif, baignoire balnéo et terrasse privée avec vue panoramique sur la vieille ville.",
                    ImagePath = "https://images.unsplash.com/photo-1578683010236-d716f9a3f461?w=800&q=80" },
                new Chambre { IdChambre = 3, Model = "Suite Présidentielle", Surface = 130, PrixParNuit = 950, Capacite = 4, Disponible = true, Etage = 6, HotelId = 1,
                    Description = "L'apogée du luxe tunisien. Deux chambres, grand salon, salle à manger privée et terrasse panoramique avec jacuzzi extérieur.",
                    ImagePath = "https://images.unsplash.com/photo-1591088398332-8596b4c8b4c5?w=800&q=80" },
                // Hasdrubal Djerba
                new Chambre { IdChambre = 4, Model = "Chambre Vue Mer", Surface = 38, PrixParNuit = 260, Capacite = 2, Disponible = true, Etage = 1, HotelId = 2,
                    Description = "Chambre lumineuse avec balcon privé et vue directe sur la Méditerranée. Décoration épurée aux tons blancs et bleus.",
                    ImagePath = "https://images.unsplash.com/photo-1615460549969-36fa19521a4f?w=800&q=80" },
                new Chambre { IdChambre = 5, Model = "Bungalow Plage", Surface = 70, PrixParNuit = 520, Capacite = 3, Disponible = true, Etage = 0, HotelId = 2,
                    Description = "Bungalow exclusif avec accès direct à la plage privée. Terrasse, salon extérieur et piscine à débordement personnelle.",
                    ImagePath = "https://images.unsplash.com/photo-1540518614846-7eded433c457?w=800&q=80" },
                // Marhaba Sousse
                new Chambre { IdChambre = 6, Model = "Chambre Deluxe", Surface = 35, PrixParNuit = 210, Capacite = 2, Disponible = true, Etage = 3, HotelId = 3,
                    Description = "Chambre deluxe avec balcon et vue sur la piscine et la mer. Équipements haut de gamme et literie premium.",
                    ImagePath = "https://images.unsplash.com/photo-1522771739844-6a9f6d5f14af?w=800&q=80" },
                new Chambre { IdChambre = 7, Model = "Suite Junior", Surface = 50, PrixParNuit = 320, Capacite = 2, Disponible = true, Etage = 5, HotelId = 3,
                    Description = "Suite junior avec vue mer panoramique, coin salon séparé et salle de bain en marbre avec douche à l'italienne.",
                    ImagePath = "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?w=800&q=80" },
                // Sindbad Hammamet
                new Chambre { IdChambre = 8, Model = "Chambre Standard", Surface = 26, PrixParNuit = 140, Capacite = 2, Disponible = true, Etage = 1, HotelId = 4,
                    Description = "Chambre confortable et bien équipée avec vue sur le jardin. Idéale pour un séjour ensoleillé à Hammamet.",
                    ImagePath = "https://images.unsplash.com/photo-1611892440504-42a792e24d32?w=800&q=80" },
                new Chambre { IdChambre = 9, Model = "Chambre Familiale", Surface = 48, PrixParNuit = 240, Capacite = 4, Disponible = true, Etage = 2, HotelId = 4,
                    Description = "Chambre spacieuse pour familles avec deux lits doubles, espace jeux et balcon donnant sur l'orangeraie.",
                    ImagePath = "https://images.unsplash.com/photo-1596394516093-501ba68a0ba6?w=800&q=80" }
            );

            // Seed amenites for rooms
            builder.Entity<ChambreAmenite>().HasData(
                new ChambreAmenite { ChambreId = 1, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 1, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 1, AmeniteId = 9 },
                new ChambreAmenite { ChambreId = 2, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 2, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 2, AmeniteId = 6 },
                new ChambreAmenite { ChambreId = 2, AmeniteId = 9 },
                new ChambreAmenite { ChambreId = 3, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 3, AmeniteId = 2 },
                new ChambreAmenite { ChambreId = 3, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 3, AmeniteId = 5 },
                new ChambreAmenite { ChambreId = 3, AmeniteId = 6 },
                new ChambreAmenite { ChambreId = 4, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 4, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 4, AmeniteId = 8 },
                new ChambreAmenite { ChambreId = 5, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 5, AmeniteId = 2 },
                new ChambreAmenite { ChambreId = 5, AmeniteId = 8 },
                new ChambreAmenite { ChambreId = 5, AmeniteId = 6 },
                new ChambreAmenite { ChambreId = 6, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 6, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 6, AmeniteId = 9 },
                new ChambreAmenite { ChambreId = 7, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 7, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 7, AmeniteId = 8 },
                new ChambreAmenite { ChambreId = 7, AmeniteId = 6 },
                new ChambreAmenite { ChambreId = 8, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 8, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 8, AmeniteId = 3 },
                new ChambreAmenite { ChambreId = 9, AmeniteId = 1 },
                new ChambreAmenite { ChambreId = 9, AmeniteId = 4 },
                new ChambreAmenite { ChambreId = 9, AmeniteId = 3 },
                new ChambreAmenite { ChambreId = 9, AmeniteId = 9 }
            );
        }
    }
}
