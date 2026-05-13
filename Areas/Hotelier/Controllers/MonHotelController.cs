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
    public class MonHotelController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public MonHotelController(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
        {
            _ctx = ctx;
            _userManager = um;
        }

        // ── Edit GET ──
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User);
            var hotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.OwnerId == userId);
            if (hotel == null) return RedirectToAction("Index", "Dashboard");
            return View(hotel);
        }

        // ── Edit POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Hotel form)
        {
            var userId = _userManager.GetUserId(User);
            var hotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.OwnerId == userId);
            if (hotel == null) return RedirectToAction("Index", "Dashboard");

            // Sécurité : on ignore l'Id du form, on garde celui en base
            ModelState.Remove("Owner");
            ModelState.Remove("Chambres");
            ModelState.Remove("Avis");

            if (!ModelState.IsValid)
            {
                // En cas d'erreur on remet les valeurs saisies
                return View(form);
            }

            // Mise à jour des champs modifiables uniquement
            hotel.Name = form.Name;
            hotel.Classification = form.Classification;
            hotel.Email = form.Email;
            hotel.NumContact = form.NumContact;
            hotel.Description = form.Description;
            hotel.Address = form.Address;
            hotel.City = form.City;
            hotel.Latitude = form.Latitude;
            hotel.Longitude = form.Longitude;
            hotel.ImagePath = form.ImagePath;

            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Informations de l'hôtel mises à jour.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}