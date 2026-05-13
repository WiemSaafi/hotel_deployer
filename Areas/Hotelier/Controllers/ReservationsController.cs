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
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationsController(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
        {
            _ctx = ctx;
            _userManager = um;
        }

        // Récupère l'hôtel du hotelier connecté (sécurité)
        private async Task<Hotel?> GetMyHotelAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _ctx.Hotels.FirstOrDefaultAsync(h => h.OwnerId == userId);
        }

        // ── Liste des réservations de l'hôtel ──
        public async Task<IActionResult> Index(string? statut = null)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            // IDs des chambres de l'hôtel
            var chambreIds = await _ctx.Chambres
                .Where(c => c.HotelId == hotel.Id)
                .Select(c => c.IdChambre)
                .ToListAsync();

            // Réservations sur ces chambres
            var query = _ctx.Reservations
                .Include(r => r.Chambre)
                .Include(r => r.Client)
                    .ThenInclude(cl => cl!.ApplicationUser)
                .Include(r => r.Paiement)
                .Where(r => chambreIds.Contains(r.ChambreId));

            // Filtre optionnel par statut
            if (!string.IsNullOrEmpty(statut))
                query = query.Where(r => r.Statut == statut);

            var reservations = await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.HotelName = hotel.Name;
            ViewBag.StatutFiltre = statut;
            ViewBag.NbEnAttente = await _ctx.Reservations
                .CountAsync(r => chambreIds.Contains(r.ChambreId) && r.Statut == "En attente");
            ViewBag.NbConfirmees = await _ctx.Reservations
                .CountAsync(r => chambreIds.Contains(r.ChambreId) && r.Statut == "Confirmée");

            return View(reservations);
        }

        // ── Confirmer une réservation ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var reservation = await _ctx.Reservations
                .Include(r => r.Chambre)
                .Include(r => r.Paiement)
                .FirstOrDefaultAsync(r => r.Id == id && r.Chambre.HotelId == hotel.Id);

            if (reservation == null) return NotFound();

            reservation.Statut = "Confirmée";
            if (reservation.Paiement != null)
                reservation.Paiement.Statut = "Validé";

            await _ctx.SaveChangesAsync();
            TempData["Success"] = $"Réservation #{reservation.Id} confirmée.";
            return RedirectToAction(nameof(Index));
        }

        // ── Détails d'une réservation ──
        public async Task<IActionResult> Details(int id)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var reservation = await _ctx.Reservations
                .Include(r => r.Chambre)
                    .ThenInclude(c => c.Hotel)
                .Include(r => r.Client)
                .Include(r => r.Paiement)
                .FirstOrDefaultAsync(r => r.Id == id && r.Chambre.HotelId == hotel.Id);

            if (reservation == null) return NotFound();

            return View(reservation);
        }

        // ── Refuser/Annuler une réservation ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Refuse(int id)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var reservation = await _ctx.Reservations
                .Include(r => r.Chambre)
                .Include(r => r.Paiement)
                .FirstOrDefaultAsync(r => r.Id == id && r.Chambre.HotelId == hotel.Id);

            if (reservation == null) return NotFound();

            reservation.Statut = "Annulée";
            if (reservation.Paiement != null)
                reservation.Paiement.Statut = "Remboursé";

            await _ctx.SaveChangesAsync();
            TempData["Success"] = $"Réservation #{reservation.Id} annulée.";
            return RedirectToAction(nameof(Index));
        }
    }
}