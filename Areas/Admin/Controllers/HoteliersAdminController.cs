using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunisiaStay.Data;
using TunisiaStay.Models;
using TunisiaStay.ViewModels;

namespace TunisiaStay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HoteliersAdminController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public HoteliersAdminController(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
        {
            _ctx = ctx;
            _userManager = um;
        }

        // ── Liste de tous les hôteliers ──
        public async Task<IActionResult> Index()
        {
            // Récupère tous les utilisateurs ayant le rôle "Hotelier"
            var hoteliers = await _userManager.GetUsersInRoleAsync("Hotelier");

            var list = new List<HotelierListItemViewModel>();
            foreach (var u in hoteliers)
            {
                var hotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.OwnerId == u.Id);
                list.Add(new HotelierListItemViewModel
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? "",
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    HotelId = hotel?.Id,
                    HotelName = hotel?.Name,
                    HotelCity = hotel?.City
                });
            }

            return View(list.OrderByDescending(h => h.CreatedAt).ToList());
        }

        // ── Create GET ──
        public async Task<IActionResult> Create()
        {
            await LoadAvailableHotelsAsync(null);
            return View(new HotelierCreateViewModel());
        }

        // ── Create POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HotelierCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await LoadAvailableHotelsAsync(vm.HotelId);
                return View(vm);
            }

            // Vérifier que l'email n'est pas déjà utilisé
            if (await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                await LoadAvailableHotelsAsync(vm.HotelId);
                return View(vm);
            }

            // Créer le compte
            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FullName = vm.FullName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                await LoadAvailableHotelsAsync(vm.HotelId);
                return View(vm);
            }

            // Lui donner le rôle Hotelier
            await _userManager.AddToRoleAsync(user, "Hotelier");

            // Lui assigner un hôtel si demandé
            if (vm.HotelId.HasValue)
            {
                var hotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.Id == vm.HotelId.Value);
                if (hotel != null && hotel.OwnerId == null)
                {
                    hotel.OwnerId = user.Id;
                    await _ctx.SaveChangesAsync();
                }
            }

            TempData["Success"] = $"Hôtelier « {vm.FullName} » créé avec succès.";
            return RedirectToAction(nameof(Index));
        }

        // ── Edit GET ──
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            if (!await _userManager.IsInRoleAsync(user, "Hotelier")) return NotFound();

            var hotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.OwnerId == user.Id);

            var vm = new HotelierEditViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                HotelId = hotel?.Id
            };

            await LoadAvailableHotelsAsync(vm.HotelId);
            return View(vm);
        }

        // ── Edit POST ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(HotelierEditViewModel vm)
        {
            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();

            // 1) Désassigner l'éventuel hôtel actuel
            var currentHotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.OwnerId == user.Id);
            if (currentHotel != null && currentHotel.Id != vm.HotelId)
            {
                currentHotel.OwnerId = null;
            }

            // 2) Assigner le nouvel hôtel (si fourni et libre)
            if (vm.HotelId.HasValue)
            {
                var newHotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.Id == vm.HotelId.Value);
                if (newHotel != null && (newHotel.OwnerId == null || newHotel.OwnerId == user.Id))
                {
                    newHotel.OwnerId = user.Id;
                }
                else
                {
                    ModelState.AddModelError("HotelId", "Cet hôtel a déjà un propriétaire.");
                    await LoadAvailableHotelsAsync(vm.HotelId);
                    return View(vm);
                }
            }

            // 3) Mise à jour du nom (l'email ne change pas, plus simple)
            user.FullName = vm.FullName;
            await _userManager.UpdateAsync(user);
            await _ctx.SaveChangesAsync();

            TempData["Success"] = "Hôtelier mis à jour.";
            return RedirectToAction(nameof(Index));
        }

        // ── Delete ──
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Désassigner son hôtel d'abord
            var hotel = await _ctx.Hotels.FirstOrDefaultAsync(h => h.OwnerId == user.Id);
            if (hotel != null)
            {
                hotel.OwnerId = null;
                await _ctx.SaveChangesAsync();
            }

            await _userManager.DeleteAsync(user);
            TempData["Success"] = "Hôtelier supprimé.";
            return RedirectToAction(nameof(Index));
        }

        // ── Helper : charger les hôtels disponibles dans ViewBag ──
        private async Task LoadAvailableHotelsAsync(int? selectedId)
        {
            // Hôtels libres (sans propriétaire) + l'hôtel actuellement sélectionné
            var hotels = await _ctx.Hotels
                .Where(h => h.OwnerId == null || h.Id == selectedId)
                .OrderBy(h => h.Name)
                .ToListAsync();

            ViewBag.AvailableHotels = new SelectList(hotels, "Id", "Name", selectedId);
        }
    }
}