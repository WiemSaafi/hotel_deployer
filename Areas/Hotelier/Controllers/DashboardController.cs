using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunisiaStay.Data;
using TunisiaStay.Models;

namespace TunisiaStay.Areas.Hotelier.Controllers
{
    [Area("Hotelier")]
    [Authorize(Roles = "Hotelier")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
        {
            _ctx = ctx;
            _userManager = um;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var hotel = await _ctx.Hotels
                .Include(h => h.Chambres)
                .FirstOrDefaultAsync(h => h.OwnerId == userId);

            if (hotel == null)
            {
                TempData["Error"] = "Aucun hôtel ne vous est assigné. Contactez l'administrateur.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var chambreIds = hotel.Chambres.Select(c => c.IdChambre).ToList();

            // ── Stats de base ──
            ViewBag.NbChambres = hotel.Chambres.Count;
            ViewBag.NbReservations = await _ctx.Reservations
                .CountAsync(r => chambreIds.Contains(r.ChambreId));
            ViewBag.NbReservationsEnAttente = await _ctx.Reservations
                .CountAsync(r => chambreIds.Contains(r.ChambreId) && r.Statut == "En attente");

            // ── Revenu total (réservations confirmées) ──
            // Note : SQLite ne supporte pas SUM(decimal), on fait la somme côté client
            var prixConfirmes = await _ctx.Reservations
                .Where(r => chambreIds.Contains(r.ChambreId) && r.Statut == "Confirmée")
                .Select(r => r.Prix)
                .ToListAsync();
            var revenuTotal = prixConfirmes.Sum();
            ViewBag.RevenuTotal = revenuTotal;
           
            // ── Réservations ce mois ──
            var debutMois = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ViewBag.NbReservationsCeMois = await _ctx.Reservations
                .CountAsync(r => chambreIds.Contains(r.ChambreId) && r.CreatedAt >= debutMois);

            // ── Revenus par mois (6 derniers mois) ──
            var sixMoisAvant = DateTime.Today.AddMonths(-5);
            var debutPeriode = new DateTime(sixMoisAvant.Year, sixMoisAvant.Month, 1);

            var reservationsPeriode = await _ctx.Reservations
                .Where(r => chambreIds.Contains(r.ChambreId)
                    && r.Statut == "Confirmée"
                    && r.CreatedAt >= debutPeriode)
                .Select(r => new { r.CreatedAt, r.Prix })
                .ToListAsync();

            // Préparer les 6 mois (même ceux à 0)
            var revenusParMois = new List<(string Mois, decimal Total)>();
            for (int i = 5; i >= 0; i--)
            {
                var moisCible = DateTime.Today.AddMonths(-i);
                var debutM = new DateTime(moisCible.Year, moisCible.Month, 1);
                var finM = debutM.AddMonths(1);

                var totalMois = reservationsPeriode
                    .Where(r => r.CreatedAt >= debutM && r.CreatedAt < finM)
                    .Sum(r => (decimal)r.Prix);

                revenusParMois.Add((debutM.ToString("MMM yy"), totalMois));
            }
            ViewBag.LabelsMois = revenusParMois.Select(r => r.Mois).ToList();
            ViewBag.RevenusMois = revenusParMois.Select(r => r.Total).ToList();

            // ── Top 3 chambres les plus réservées ──
            var topChambres = await _ctx.Reservations
                .Where(r => chambreIds.Contains(r.ChambreId))
                .GroupBy(r => r.ChambreId)
                .Select(g => new { ChambreId = g.Key, Nb = g.Count() })
                .OrderByDescending(x => x.Nb)
                .Take(3)
                .ToListAsync();

            var topChambresDetails = topChambres
                .Select(t => new
                {
                    Chambre = hotel.Chambres.FirstOrDefault(c => c.IdChambre == t.ChambreId),
                    NbReservations = t.Nb
                })
                .Where(x => x.Chambre != null)
                .ToList();
            ViewBag.TopChambres = topChambresDetails;

            return View(hotel);
        }
    }
}