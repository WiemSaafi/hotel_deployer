using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunisiaStay.Data;
using TunisiaStay.Models;
using TunisiaStay.Services;
using TunisiaStay.ViewModels;

namespace TunisiaStay.Controllers
{
    // ══ Home ══════════════════════════════════════════════════════════
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _uow;
        public HomeController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index(string? City, DateTime? DateArrivee, DateTime? DateDepart, int? Capacite, decimal? PrixMax, string? EtoilesMin)
        {
            var hotels = (await _uow.Hotels.GetHotelsWithChambresAsync()).ToList();

            if (!string.IsNullOrEmpty(City))
                hotels = hotels.Where(h => h.City == City).ToList();

            if (!string.IsNullOrEmpty(EtoilesMin))
                hotels = hotels.Where(h => h.Classification == EtoilesMin).ToList();

            if (PrixMax.HasValue)
                hotels = hotels.Where(h => h.Chambres.Any(c => c.PrixParNuit <= PrixMax.Value)).ToList();

            if (Capacite.HasValue)
                hotels = hotels.Where(h => h.Chambres.Any(c => c.Capacite >= Capacite.Value)).ToList();

            if (DateArrivee.HasValue && DateDepart.HasValue)
                hotels = hotels.Where(h => h.Chambres.Any(c =>
                    c.Disponible &&
                    !c.Reservations.Any(r =>
                        (r.Statut == "En attente" || r.Statut == "Confirmée") &&
                        r.Date < DateDepart.Value &&
                        r.Date.AddDays(r.Durée) > DateArrivee.Value
                    )
                )).ToList();

            return View(hotels);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View();
    }

    // ══ Hotels (public) ═══════════════════════════════════════════════
    public class HotelsController : Controller
    {
        private readonly IUnitOfWork _uow;
        public HotelsController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index(string? City, DateTime? DateArrivee, DateTime? DateDepart, int? Capacite)
        {
            var hotels = (await _uow.Hotels.GetHotelsWithChambresAsync()).ToList();

            if (!string.IsNullOrEmpty(City))
                hotels = hotels.Where(h => h.City == City).ToList();

            if (Capacite.HasValue)
                hotels = hotels.Where(h => h.Chambres.Any(c => c.Capacite >= Capacite.Value)).ToList();

            if (DateArrivee.HasValue && DateDepart.HasValue)
                hotels = hotels.Where(h => h.Chambres.Any(c =>
                    c.Disponible &&
                    !c.Reservations.Any(r =>
                        (r.Statut == "En attente" || r.Statut == "Confirmée") &&
                        r.Date < DateDepart.Value &&
                        r.Date.AddDays(r.Durée) > DateArrivee.Value
                    )
                )).ToList();

            return View(hotels);
        }

        public async Task<IActionResult> Details(int id)
        {
            var hotel = await _uow.Hotels.GetHotelDetailAsync(id);
            if (hotel == null) return NotFound();
            return View(hotel);
        }
    }

    // ══ Reservations (requires login) ═════════════════════════════════
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _ctx;

        public ReservationsController(IUnitOfWork uow,
            UserManager<ApplicationUser> um, ApplicationDbContext ctx)
        { _uow = uow; _userManager = um; _ctx = ctx; }

        // ── Create GET ─────────────────────────────────────────────────
        public async Task<IActionResult> Create(int chambreId)
        {
            var chambre = await _uow.Chambres.GetChambreDetailAsync(chambreId);
            if (chambre == null) return NotFound();

            // Récupérer les périodes déjà réservées (pour info dans la vue)
            var aujourdhui = DateTime.Today;
            var reservationsExistantes = (await _ctx.Reservations
                .Where(r => r.ChambreId == chambreId
                    && (r.Statut == "En attente" || r.Statut == "Confirmée"))
                .Select(r => new { r.Date, r.Durée })
                .ToListAsync())
                .Where(r => r.Date.AddDays(r.Durée) >= aujourdhui)
                .OrderBy(r => r.Date)
                .Select(r => new PeriodeReserveeViewModel { Debut = r.Date, Fin = r.Date.AddDays(r.Durée) })
                .ToList();

            ViewBag.PeriodesReservees = reservationsExistantes;

            // Charger les offres actives de l'hôtel (toutes celles en cours)
            var offresActives = await _ctx.Offres
                .Where(o => o.HotelId == chambre.HotelId
                    && o.IsActive
                    && o.DateFin >= aujourdhui)
                .OrderByDescending(o => o.Pourcentage)
                .ToListAsync();

            ViewBag.OffresActives = offresActives;

            return View(new ReservationCreateViewModel
            {
                ChambreId = chambreId,
                Chambre = chambre,
                DateArrivee = DateTime.Today,
                DateDepart  = DateTime.Today.AddDays(1)
            });
        }
        
        // ── Create POST ────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel vm)
        {
            if (vm.DateDepart <= vm.DateArrivee)
                ModelState.AddModelError("", "La date de départ doit être après la date d'arrivée.");

            // Vérifier que la date d'arrivée n'est pas dans le passé
            if (vm.DateArrivee < DateTime.Today)
                ModelState.AddModelError("", "La date d'arrivée ne peut pas être dans le passé.");

            // Vérifier que la chambre est marquée comme disponible
            var chambreCheck = await _uow.Chambres.GetByIdAsync(vm.ChambreId);
            if (chambreCheck == null)
                ModelState.AddModelError("", "Chambre introuvable.");
            else if (!chambreCheck.Disponible)
                ModelState.AddModelError("", "Cette chambre n'est pas disponible actuellement.");

            // Vérifier qu'il n'y a pas de conflit avec d'autres réservations
            // (formule de chevauchement : début_A < fin_B ET fin_A > début_B)
            if (vm.DateDepart > vm.DateArrivee)  // évite les calculs si dates invalides
            {
                var reservationsActives = await _ctx.Reservations
                    .Where(r => r.ChambreId == vm.ChambreId
                        && (r.Statut == "En attente" || r.Statut == "Confirmée"))
                    .Select(r => new { r.Date, r.Durée })
                    .ToListAsync();

                var conflitExiste = reservationsActives.Any(r =>
                    r.Date < vm.DateDepart && r.Date.AddDays(r.Durée) > vm.DateArrivee);

                if (conflitExiste)
                    ModelState.AddModelError("", "Cette chambre est déjà réservée sur ces dates. Choisissez d'autres dates.");
            }

            if (!ModelState.IsValid)
            {
                vm.Chambre = await _uow.Chambres.GetChambreDetailAsync(vm.ChambreId);

                // Recharger les périodes réservées pour ré-afficher la vue
                var aujourdhuiCheck = DateTime.Today;
                ViewBag.PeriodesReservees = (await _ctx.Reservations
                    .Where(r => r.ChambreId == vm.ChambreId
                        && (r.Statut == "En attente" || r.Statut == "Confirmée"))
                    .Select(r => new { r.Date, r.Durée })
                    .ToListAsync())
                    .Where(r => r.Date.AddDays(r.Durée) >= aujourdhuiCheck)
                    .OrderBy(r => r.Date)
                    .Select(r => new PeriodeReserveeViewModel { Debut = r.Date, Fin = r.Date.AddDays(r.Durée) })
                    .ToList();

                return View(vm);
            }

            var user   = await _userManager.GetUserAsync(User);
            var client = await _uow.Clients.GetByUserIdAsync(user!.Id);
            if (client == null)
                return RedirectToAction("Login", "Account");

            var chambre = await _uow.Chambres.GetByIdAsync(vm.ChambreId);
            if (chambre == null) return NotFound();

            // Charger les offres actives de l'hôtel pour la période
            var offresHotel = await _ctx.Offres
                .Where(o => o.HotelId == chambre.HotelId)
                .ToListAsync();

            var meilleureOffre = TarificationHelper.TrouverMeilleureOffre(
                offresHotel, vm.DateArrivee, vm.DateDepart);

            var durée = (vm.DateDepart - vm.DateArrivee).TotalDays;
            var prixSansPromo = chambre.PrixParNuit * (decimal)durée;

            // Appliquer la réduction si une offre est active
            var prix = meilleureOffre != null
                ? TarificationHelper.AppliquerReduction(prixSansPromo, meilleureOffre.Pourcentage)
                : prixSansPromo;


            var reservation = new Reservation
            {
                Date     = vm.DateArrivee,
                Durée    = durée,
                Prix     = (double)prix,
                Statut   = "En attente",
                ChambreId = vm.ChambreId,
                ClientId  = client.ClientKey,
                Paiement = new Paiement
                {
                    Montant      = prix,
                    Méthode      = vm.MethodePaiement,
                    Statut       = "En attente",
                    DatePaiement = DateTime.UtcNow
                }
            };

            await _uow.Reservations.AddAsync(reservation);
            await _uow.CompleteAsync();

            TempData["Success"] = $"Réservation créée avec succès ! Référence : #{reservation.Id}";
            return RedirectToAction("MyReservations");
        }

        // ── My Reservations ─────────────────────────────────────────────
        public async Task<IActionResult> MyReservations()
        {
            var user   = await _userManager.GetUserAsync(User);
            var client = await _uow.Clients.GetByUserIdAsync(user!.Id);
            if (client == null) return View(new List<Reservation>());
            var reservations = await _uow.Reservations.GetByClientAsync(client.ClientKey);
            return View(reservations);
        }

        // ── Cancel ──────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _uow.Reservations.GetDetailAsync(id);
            if (reservation == null) return NotFound();

            var user   = await _userManager.GetUserAsync(User);
            var client = await _uow.Clients.GetByUserIdAsync(user!.Id);
            if (client == null || reservation.ClientId != client.ClientKey)
                return Forbid();

            reservation.Statut = "Annulée";
            if (reservation.Paiement != null)
                reservation.Paiement.Statut = "Remboursé";

            _uow.Reservations.Update(reservation);
            await _uow.CompleteAsync();
            TempData["Success"] = "Réservation annulée.";
            return RedirectToAction("MyReservations");
        }
    }

    // ══ Account ════════════════════════════════════════════════════════
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IUnitOfWork _uow;
        private readonly ApplicationDbContext _ctx;

        public AccountController(UserManager<ApplicationUser> um,
            SignInManager<ApplicationUser> sm,
            IUnitOfWork uow,
            ApplicationDbContext ctx)
        { _userManager = um; _signInManager = sm; _uow = uow; _ctx = ctx; }

        // ── Login ───────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(vm.Email);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });

                return LocalRedirect(returnUrl ?? "/");
            }

            ModelState.AddModelError("", "Email ou mot de passe incorrect.");
            return View(vm);
        }

        // ── Register ────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName       = vm.Email,
                Email          = vm.Email,
                FullName       = vm.FullName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "User");

                var parts  = vm.FullName.Split(' ', 2);
                var client = new Client
                {
                    Email             = vm.Email,
                    Telephone         = vm.Telephone,
                    Numéro            = vm.Numéro,
                    Prénom            = parts[0],
                    Nom               = parts.Length > 1 ? parts[1] : "",
                    ApplicationUserId = user.Id
                };
                _ctx.Clients.Add(client);
                await _ctx.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var e in result.Errors)
                ModelState.AddModelError("", e.Description);
            return View(vm);
        }

        // ── Logout ──────────────────────────────────────────────────────
        [HttpPost,ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ── Profile ─────────────────────────────────────────────────────
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user   = await _userManager.GetUserAsync(User);
            var client = await _uow.Clients.GetByUserIdAsync(user!.Id);
            return View((user, client));
        }

        public IActionResult AccessDenied() => View();
    }

    // ══ Chambres (public) ═════════════════════════════════════════════
    public class ChambresController : Controller
    {
        private readonly IUnitOfWork _uow;
        public ChambresController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index(int? hotelId, string? city, string? model,
    decimal? min, decimal? max, int? cap,
    DateTime? arrival, DateTime? depart)
        {
            var chambres = await _uow.Chambres.SearchAsync(city, model, min, max, cap, arrival, depart);

            if (hotelId.HasValue)
                chambres = chambres.Where(c => c.HotelId == hotelId.Value).ToList();

            ViewBag.HotelId = hotelId;
            ViewBag.HotelNom = chambres.FirstOrDefault()?.Hotel?.Name ?? "";
            return View(chambres);
        }
    }
}
