using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TunisiaStay.Data;
using TunisiaStay.Models;

namespace TunisiaStay.ViewComponents
{
    public class HotelierMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _ctx;
        private readonly UserManager<ApplicationUser> _userManager;

        public HotelierMenuViewComponent(ApplicationDbContext ctx, UserManager<ApplicationUser> um)
        {
            _ctx = ctx;
            _userManager = um;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // L'utilisateur connecté
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            if (user == null) return View(0);

            // Son hôtel
            var hotel = await _ctx.Hotels
                .FirstOrDefaultAsync(h => h.OwnerId == user.Id);
            if (hotel == null) return View(0);

            // Compter ses réservations en attente
            var nbEnAttente = await _ctx.Reservations
                .CountAsync(r => r.Chambre.HotelId == hotel.Id && r.Statut == "En attente");

            return View(nbEnAttente);
        }
    }
}