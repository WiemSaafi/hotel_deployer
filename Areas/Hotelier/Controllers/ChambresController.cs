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
    public class ChambresController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChambresController(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
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

        // ── Liste ──
        public async Task<IActionResult> Index()
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var chambres = await _ctx.Chambres
                .Where(c => c.HotelId == hotel.Id)
                .ToListAsync();

            ViewBag.HotelName = hotel.Name;
            return View(chambres);
        }

        // ── Create GET ──
        public async Task<IActionResult> Create()
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");
            ViewBag.HotelName = hotel.Name;
            return View(new Chambre { HotelId = hotel.Id, Disponible = true });
        }

        // ── Create POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Chambre chambre)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            // Sécurité : on force le HotelId du hotelier connecté
            chambre.HotelId = hotel.Id;
            ModelState.Remove("Hotel");

            if (!ModelState.IsValid)
            {
                ViewBag.HotelName = hotel.Name;
                return View(chambre);
            }

            _ctx.Chambres.Add(chambre);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = $"Chambre « {chambre.Model} » ajoutée.";
            return RedirectToAction(nameof(Index));
        }

        // ── Edit GET ──
        public async Task<IActionResult> Edit(int id)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var chambre = await _ctx.Chambres
                .FirstOrDefaultAsync(c => c.IdChambre == id && c.HotelId == hotel.Id);
            if (chambre == null) return NotFound();

            ViewBag.HotelName = hotel.Name;
            return View(chambre);
        }

        // ── Edit POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Chambre chambre)
        {
            if (id != chambre.IdChambre) return BadRequest();

            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            // Sécurité : la chambre doit appartenir à SON hôtel
            var existing = await _ctx.Chambres
                .FirstOrDefaultAsync(c => c.IdChambre == id && c.HotelId == hotel.Id);
            if (existing == null) return NotFound();

            ModelState.Remove("Hotel");
            if (!ModelState.IsValid)
            {
                ViewBag.HotelName = hotel.Name;
                return View(chambre);
            }

            existing.Model = chambre.Model;
            existing.Surface = chambre.Surface;
            existing.PrixParNuit = chambre.PrixParNuit;
            existing.Capacite = chambre.Capacite;
            existing.Etage = chambre.Etage;
            existing.Description = chambre.Description;
            existing.ImagePath = chambre.ImagePath;
            existing.Disponible = chambre.Disponible;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Chambre mise à jour.";
            return RedirectToAction(nameof(Index));
        }

        // ── Delete ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var chambre = await _ctx.Chambres
                .FirstOrDefaultAsync(c => c.IdChambre == id && c.HotelId == hotel.Id);
            if (chambre == null) return NotFound();

            _ctx.Chambres.Remove(chambre);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Chambre supprimée.";
            return RedirectToAction(nameof(Index));
        }
    }
}