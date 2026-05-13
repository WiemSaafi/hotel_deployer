using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TunisiaStay.Models
{
    // ─────────────────────────────────────────────────────────────────
    //  IDENTITY ACCOUNT  (extends IdentityUser)
    // ─────────────────────────────────────────────────────────────────
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public Client? Client { get; set; }

        public ICollection<Hotel> OwnedHotels { get; set; } = new List<Hotel>();

    }

    // ─────────────────────────────────────────────────────────────────
    //  HOTEL
    // ─────────────────────────────────────────────────────────────────
    public class Hotel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Classification { get; set; } = string.Empty;   // e.g. "5 étoiles"

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int NumContact { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;             // Tunis, Sousse, Djerba …

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Chambre> Chambres { get; set; } = new List<Chambre>();
        public ICollection<Avis> Avis { get; set; } = new List<Avis>();

        public string? OwnerId { get; set; }
        public ApplicationUser? Owner { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    //  CHAMBRE
    // ─────────────────────────────────────────────────────────────────
    public class Chambre
    {
        public int IdChambre { get; set; }

        [Required, StringLength(100)]
        public string Model { get; set; } = string.Empty;           // "Deluxe", "Suite", etc.

        [Range(10, 500)]
        public int Surface { get; set; }                            // m²

        [Column(TypeName = "decimal(18,2)")]
        public decimal PrixParNuit { get; set; }

        public int Capacite { get; set; } = 1;
        public bool Disponible { get; set; } = true;
        public int Etage { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        // FK → Hotel
        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public ICollection<ChambreAmenite> ChambreAmenites { get; set; } = new List<ChambreAmenite>();

        // Offres promotionnelles de l'hôtel
        public ICollection<Offre> Offres { get; set; } = new List<Offre>();
    }
    
    

    // ─────────────────────────────────────────────────────────────────
    //  CLIENT
    // ─────────────────────────────────────────────────────────────────
    public class Client
    {
        public int ClientKey { get; set; }

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int Numéro { get; set; }

        [Required]
        public int Telephone { get; set; }

        [StringLength(100)]
        public string Prénom { get; set; } = string.Empty;

        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        // FK → ApplicationUser (1-1)
        public string? ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    // ─────────────────────────────────────────────────────────────────
    //  RESERVATION
    // ─────────────────────────────────────────────────────────────────
    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public double Durée { get; set; }                           // nuits

        [Column(TypeName = "decimal(18,2)")]
        public double Prix { get; set; }

        public DateTime DateFin => Date.AddDays(Durée);

        [StringLength(50)]
        public string Statut { get; set; } = "En attente";         // En attente / Confirmée / Annulée

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FKs
        public int ChambreId { get; set; }
        public Chambre Chambre { get; set; } = null!;

        public int ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Paiement? Paiement { get; set; }
    }

    // ─────────────────────────────────────────────────────────────────
    //  PAIEMENT  (extra table)
    // ─────────────────────────────────────────────────────────────────
    public class Paiement
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Montant { get; set; }

        [StringLength(50)]
        public string Méthode { get; set; } = "Carte";             // Carte / Espèces / Virement

        [StringLength(50)]
        public string Statut { get; set; } = "En attente";

        public DateTime DatePaiement { get; set; } = DateTime.UtcNow;

        // FK → Reservation (1-1)
        public int ReservationId { get; set; }
        public Reservation Reservation { get; set; } = null!;
    }

    // ─────────────────────────────────────────────────────────────────
    //  AMENITE  (extra table – équipements)
    // ─────────────────────────────────────────────────────────────────
    public class Amenite
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nom { get; set; } = string.Empty;            // WiFi, Piscine, Parking …

        public string? IconClass { get; set; }                     // "fa-wifi"

        public ICollection<ChambreAmenite> ChambreAmenites { get; set; } = new List<ChambreAmenite>();
    }

    public class ChambreAmenite
    {
        public int ChambreId { get; set; }
        public Chambre Chambre { get; set; } = null!;

        public int AmeniteId { get; set; }
        public Amenite Amenite { get; set; } = null!;
    }

    // ─────────────────────────────────────────────────────────────────
    //  AVIS  (extra table – reviews)
    // ─────────────────────────────────────────────────────────────────
    public class Avis
    {
        public int Id { get; set; }

        [Range(1, 5)]
        public int Note { get; set; }

        [StringLength(1000)]
        public string Commentaire { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public int HotelId { get; set; }
        public Hotel Hotel { get; set; } = null!;

        public string ApplicationUserId { get; set; } = string.Empty;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }

    // ── Offre promotionnelle ──
    public class Offre
    {
        public int Id { get; set; }

        [Required]
        public string Titre { get; set; } = "";

        public string? Description { get; set; }

        [Range(1, 90, ErrorMessage = "Le pourcentage doit être entre 1 et 90")]
        public int Pourcentage { get; set; }   // ex: 20 = -20%

        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // FK vers Hotel
        public int HotelId { get; set; }
        public Hotel? Hotel { get; set; }
    }

    // ── Helper pour calculer le prix avec offre active ──
    public static class TarificationHelper
    {
        /// <summary>
        /// Trouve l'offre la plus avantageuse active sur la période donnée pour un hôtel.
        /// Retourne null si aucune offre n'est active.
        /// </summary>
        public static Offre? TrouverMeilleureOffre(IEnumerable<Offre> offres, DateTime dateArrivee, DateTime dateDepart)
        {
            return offres
                .Where(o => o.IsActive
                    && o.DateDebut <= dateDepart
                    && o.DateFin >= dateArrivee)
                .OrderByDescending(o => o.Pourcentage)
                .FirstOrDefault();
        }

        /// <summary>
        /// Applique le pourcentage de réduction au prix.
        /// </summary>
        public static decimal AppliquerReduction(decimal prixOriginal, int pourcentage)
        {
            return Math.Round(prixOriginal * (100 - pourcentage) / 100m, 2);
        }
    }
}
