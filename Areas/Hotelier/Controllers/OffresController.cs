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
    public class OffresController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public OffresController(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
        {
            _ctx = ctx;
            _userManager = um;
        }

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

            var offres = await _ctx.Offres
                .Where(o => o.HotelId == hotel.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            ViewBag.HotelName = hotel.Name;
            return View(offres);
        }

        // ── Create GET ──
        public async Task<IActionResult> Create()
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");
            ViewBag.HotelName = hotel.Name;
            return View(new Offre
            {
                HotelId = hotel.Id,
                DateDebut = DateTime.Today,
                DateFin = DateTime.Today.AddDays(30),
                IsActive = true
            });
        }

        // ── Create POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Offre offre)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            offre.HotelId = hotel.Id;
            ModelState.Remove("Hotel");

            if (offre.DateFin <= offre.DateDebut)
                ModelState.AddModelError("", "La date de fin doit être après la date de début.");

            if (!ModelState.IsValid)
            {
                ViewBag.HotelName = hotel.Name;
                return View(offre);
            }

            offre.CreatedAt = DateTime.UtcNow;
            _ctx.Offres.Add(offre);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = $"Offre « {offre.Titre} » créée.";
            return RedirectToAction(nameof(Index));
        }

        // ── Edit GET ──
        public async Task<IActionResult> Edit(int id)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var offre = await _ctx.Offres
                .FirstOrDefaultAsync(o => o.Id == id && o.HotelId == hotel.Id);
            if (offre == null) return NotFound();

            ViewBag.HotelName = hotel.Name;
            return View(offre);
        }

        // ── Edit POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Offre offre)
        {
            if (id != offre.Id) return BadRequest();

            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var existing = await _ctx.Offres
                .FirstOrDefaultAsync(o => o.Id == id && o.HotelId == hotel.Id);
            if (existing == null) return NotFound();

            ModelState.Remove("Hotel");

            if (offre.DateFin <= offre.DateDebut)
                ModelState.AddModelError("", "La date de fin doit être après la date de début.");

            if (!ModelState.IsValid)
            {
                ViewBag.HotelName = hotel.Name;
                return View(offre);
            }

            existing.Titre = offre.Titre;
            existing.Description = offre.Description;
            existing.Pourcentage = offre.Pourcentage;
            existing.DateDebut = offre.DateDebut;
            existing.DateFin = offre.DateFin;
            existing.IsActive = offre.IsActive;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Offre mise à jour.";
            return RedirectToAction(nameof(Index));
        }

        // ── Delete ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var hotel = await GetMyHotelAsync();
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            var offre = await _ctx.Offres
                .FirstOrDefaultAsync(o => o.Id == id && o.HotelId == hotel.Id);
            if (offre == null) return NotFound();

            _ctx.Offres.Remove(offre);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Offre supprimée.";
            return RedirectToAction(nameof(Index));
        }
    }
}