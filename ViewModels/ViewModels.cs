using System.ComponentModel.DataAnnotations;
using TunisiaStay.Models;

namespace TunisiaStay.ViewModels
{
    // ── Auth ──────────────────────────────────────────────────────────
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Minimum 6 caractères")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veuillez confirmer le mot de passe")]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public int Telephone { get; set; }
        public int Numéro { get; set; }
    }

    // ── Room Search ───────────────────────────────────────────────────
    public class RoomSearchViewModel
    {
        public string? City { get; set; }
        public string? Model { get; set; }
        public decimal? PrixMin { get; set; }
        public decimal? PrixMax { get; set; }
        public int? Capacite { get; set; }
        public DateTime? DateArrivee { get; set; }
        public DateTime? DateDepart { get; set; }
        public List<Chambre> Results { get; set; } = new();
        public List<string> Cities { get; set; } = new();
    }

    // ── Reservation ───────────────────────────────────────────────────
    public class ReservationCreateViewModel
    {
        public int ChambreId { get; set; }
        public Chambre? Chambre { get; set; }

        [Required(ErrorMessage = "La date d'arrivée est requise")]
        public DateTime DateArrivee { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "La date de départ est requise")]
        public DateTime DateDepart { get; set; } = DateTime.Today.AddDays(1);

        public string MethodePaiement { get; set; } = "Carte";
    }

    // ── Statistics ────────────────────────────────────────────────────
    public class StatisticsViewModel
    {
        public int TotalHotels { get; set; }
        public int TotalChambres { get; set; }
        public int TotalClients { get; set; }
        public int TotalReservations { get; set; }
        public decimal RevenuTotal { get; set; }
        public int ReservationsEnAttente { get; set; }
        public int ReservationsConfirmees { get; set; }
        public int ReservationsAnnulees { get; set; }
        public List<(string City, int Count)> ReservationsParVille { get; set; } = new();
        public List<Hotel> TopHotels { get; set; } = new();
    }

    // ── Hotel Form ────────────────────────────────────────────────────
    public class HotelFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La classification est requise")]
        [StringLength(50)]
        public string Classification { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le numéro de contact est requis")]
        public int NumContact { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public IFormFile? Image { get; set; }
    }

    // ── Chambre Form ──────────────────────────────────────────────────
    public class ChambreFormViewModel
    {
        public int IdChambre { get; set; }

        [Required(ErrorMessage = "Le type est requis")]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;

        [Range(10, 500, ErrorMessage = "Surface entre 10 et 500 m²")]
        public int Surface { get; set; }

        [Range(1, 10000, ErrorMessage = "Prix invalide")]
        public decimal PrixParNuit { get; set; }

        [Range(1, 20)]
        public int Capacite { get; set; } = 1;

        public int Etage { get; set; }

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool Disponible { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner un hôtel")]
        public int HotelId { get; set; }

        public IFormFile? Image { get; set; }
        public List<int> AmeniteIds { get; set; } = new();
        public List<Amenite> AllAmenites { get; set; } = new();
        public List<Hotel> Hotels { get; set; } = new();
    }

    // ── ViewModels pour la gestion des hôteliers (admin) ──

    public class HotelierListItemViewModel
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? HotelId { get; set; }
        public string? HotelName { get; set; }
        public string? HotelCity { get; set; }
    }

    public class HotelierCreateViewModel
    {
        [Required(ErrorMessage = "Le nom complet est requis")]
        public string FullName { get; set; } = "";

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [MinLength(6, ErrorMessage = "Minimum 6 caractères")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public int? HotelId { get; set; }   // optionnel à la création
    }

    public class HotelierEditViewModel
    {
        public string Id { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public int? HotelId { get; set; }
    }

    public class PeriodeReserveeViewModel
    {
        public DateTime Debut { get; set; }
        public DateTime Fin { get; set; }
    }

    // ── Dashboard Hotelier ────────────────────────────────────────────
    public class ChambreTopDto
    {
        public Chambre Chambre { get; set; } = null!;
        public int NbReservations { get; set; }
    }

    // ── Client Form (Admin CRUD) ───────────────────────────────────────
    public class ClientFormViewModel
    {
        public int ClientKey { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(100)]
        public string Prénom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        public int Telephone { get; set; }
        public int Numéro { get; set; }
    }
}
