using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TunisiaStay.Data;
using TunisiaStay.Models;
using TunisiaStay.Services;
using TunisiaStay.ViewModels;

namespace TunisiaStay.Areas.Admin.Controllers
{
    // ── Dashboard ─────────────────────────────────────────────────────
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IStatisticsService _stats;
        public DashboardController(IStatisticsService stats) => _stats = stats;

        public async Task<IActionResult> Index()
            => View(await _stats.GetStatisticsAsync());
    }

    // ── Hotels CRUD ───────────────────────────────────────────────────
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class HotelsAdminController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _imgSvc;

        public HotelsAdminController(IUnitOfWork uow, IImageService imgSvc)
        { _uow = uow; _imgSvc = imgSvc; }

        public async Task<IActionResult> Index()
            => View(await _uow.Hotels.GetHotelsWithChambresAsync());

        public IActionResult Create() => View(new HotelFormViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HotelFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var hotel = new Hotel
            {
                Name = vm.Name, Classification = vm.Classification,
                Email = vm.Email, NumContact = vm.NumContact,
                Description = vm.Description, Address = vm.Address,
                City = vm.City, Latitude = vm.Latitude, Longitude = vm.Longitude
            };
            if (vm.Image != null)
                hotel.ImagePath = await _imgSvc.SaveImageAsync(vm.Image, "hotels");
            await _uow.Hotels.AddAsync(hotel);
            await _uow.CompleteAsync();
            TempData["Success"] = "Hôtel créé avec succès.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var h = await _uow.Hotels.GetByIdAsync(id);
            if (h == null) return NotFound();
            return View(new HotelFormViewModel
            {
                Id = h.Id, Name = h.Name, Classification = h.Classification,
                Email = h.Email, NumContact = h.NumContact,
                Description = h.Description, Address = h.Address,
                City = h.City, Latitude = h.Latitude, Longitude = h.Longitude
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HotelFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var h = await _uow.Hotels.GetByIdAsync(id);
            if (h == null) return NotFound();
            h.Name = vm.Name; h.Classification = vm.Classification;
            h.Email = vm.Email; h.NumContact = vm.NumContact;
            h.Description = vm.Description; h.Address = vm.Address;
            h.City = vm.City; h.Latitude = vm.Latitude; h.Longitude = vm.Longitude;
            if (vm.Image != null)
            {
                _imgSvc.DeleteImage(h.ImagePath);
                h.ImagePath = await _imgSvc.SaveImageAsync(vm.Image, "hotels");
            }
            _uow.Hotels.Update(h);
            await _uow.CompleteAsync();
            TempData["Success"] = "Hôtel mis à jour.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var h = await _uow.Hotels.GetByIdAsync(id);
            if (h == null) return NotFound();
            _imgSvc.DeleteImage(h.ImagePath);
            _uow.Hotels.Delete(h);
            await _uow.CompleteAsync();
            TempData["Success"] = "Hôtel supprimé.";
            return RedirectToAction("Index");
        }
    }

    // ── Chambres CRUD ─────────────────────────────────────────────────
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ChambresAdminController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IImageService _imgSvc;
        private readonly ApplicationDbContext _ctx;

        public ChambresAdminController(IUnitOfWork uow, IImageService imgSvc, ApplicationDbContext ctx)
        { _uow = uow; _imgSvc = imgSvc; _ctx = ctx; }

        public async Task<IActionResult> Index()
            => View(await _ctx.Chambres.Include(c => c.Hotel).ToListAsync());

        private async Task<ChambreFormViewModel> BuildForm(ChambreFormViewModel? vm = null)
        {
            vm ??= new ChambreFormViewModel();
            vm.Hotels = (await _uow.Hotels.GetAllAsync()).ToList();
            vm.AllAmenites = await _ctx.Amenites.ToListAsync();
            return vm;
        }

        public async Task<IActionResult> Create() => View(await BuildForm());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ChambreFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(await BuildForm(vm));
            var chambre = new Chambre
            {
                Model = vm.Model, Surface = vm.Surface, PrixParNuit = vm.PrixParNuit,
                Capacite = vm.Capacite, Etage = vm.Etage, Description = vm.Description,
                Disponible = vm.Disponible, HotelId = vm.HotelId
            };
            if (vm.Image != null)
                chambre.ImagePath = await _imgSvc.SaveImageAsync(vm.Image, "chambres");
            _ctx.Chambres.Add(chambre);
            await _ctx.SaveChangesAsync();
            foreach (var aid in vm.AmeniteIds)
                _ctx.ChambreAmenites.Add(new ChambreAmenite { ChambreId = chambre.IdChambre, AmeniteId = aid });
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Chambre créée.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(int id)
        {
            var c = await _ctx.Chambres.Include(x => x.ChambreAmenites).FirstOrDefaultAsync(x => x.IdChambre == id);
            if (c == null) return NotFound();
            var vm = await BuildForm(new ChambreFormViewModel
            {
                IdChambre = c.IdChambre, Model = c.Model, Surface = c.Surface,
                PrixParNuit = c.PrixParNuit, Capacite = c.Capacite,
                Etage = c.Etage, Description = c.Description,
                Disponible = c.Disponible, HotelId = c.HotelId,
                AmeniteIds = c.ChambreAmenites.Select(x => x.AmeniteId).ToList()
            });
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ChambreFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(await BuildForm(vm));
            var c = await _ctx.Chambres.Include(x => x.ChambreAmenites).FirstOrDefaultAsync(x => x.IdChambre == id);
            if (c == null) return NotFound();
            c.Model = vm.Model; c.Surface = vm.Surface; c.PrixParNuit = vm.PrixParNuit;
            c.Capacite = vm.Capacite; c.Etage = vm.Etage; c.Description = vm.Description;
            c.Disponible = vm.Disponible; c.HotelId = vm.HotelId;
            if (vm.Image != null)
            {
                _imgSvc.DeleteImage(c.ImagePath);
                c.ImagePath = await _imgSvc.SaveImageAsync(vm.Image, "chambres");
            }
            _ctx.ChambreAmenites.RemoveRange(c.ChambreAmenites);
            foreach (var aid in vm.AmeniteIds)
                _ctx.ChambreAmenites.Add(new ChambreAmenite { ChambreId = c.IdChambre, AmeniteId = aid });
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Chambre mise à jour.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _uow.Chambres.GetByIdAsync(id);
            if (c == null) return NotFound();
            _imgSvc.DeleteImage(c.ImagePath);
            _uow.Chambres.Delete(c);
            await _uow.CompleteAsync();
            TempData["Success"] = "Chambre supprimée.";
            return RedirectToAction("Index");
        }
    }

    // ── Reservations Admin ────────────────────────────────────────────
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ReservationsAdminController : Controller
    {
        private readonly IUnitOfWork _uow;
        public ReservationsAdminController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index()
            => View(await _uow.Reservations.GetAllWithDetailsAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string statut)
        {
            var r = await _uow.Reservations.GetDetailAsync(id);
            if (r == null) return NotFound();
            r.Statut = statut;
            if (r.Paiement != null && statut == "Confirmée")
                r.Paiement.Statut = "Payé";
            _uow.Reservations.Update(r);
            await _uow.CompleteAsync();
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var r = await _uow.Reservations.GetByIdAsync(id);
            if (r == null) return NotFound();
            _uow.Reservations.Delete(r);
            await _uow.CompleteAsync();
            TempData["Success"] = "Réservation supprimée.";
            return RedirectToAction("Index");
        }
    }

    // ── Clients Admin ─────────────────────────────────────────────────
    [Area("Admin"), Authorize(Roles = "Admin")]
    public class ClientsAdminController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public ClientsAdminController(ApplicationDbContext ctx) => _ctx = ctx;

        public async Task<IActionResult> Index()
            => View(await _ctx.Clients.Include(c => c.Reservations).ToListAsync());

        // GET: Create
        public IActionResult Create() => View(new ClientFormViewModel());

        // POST: Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var client = new TunisiaStay.Models.Client
            {
                Prénom    = vm.Prénom,
                Nom       = vm.Nom,
                Email     = vm.Email,
                Telephone = vm.Telephone,
                Numéro    = vm.Numéro
            };
            _ctx.Clients.Add(client);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Client créé avec succès.";
            return RedirectToAction("Index");
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _ctx.Clients.FindAsync(id);
            if (c == null) return NotFound();
            return View(new ClientFormViewModel
            {
                ClientKey = c.ClientKey,
                Prénom    = c.Prénom,
                Nom       = c.Nom,
                Email     = c.Email,
                Telephone = c.Telephone,
                Numéro    = c.Numéro
            });
        }

        // POST: Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClientFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);
            var c = await _ctx.Clients.FindAsync(vm.ClientKey);
            if (c == null) return NotFound();
            c.Prénom    = vm.Prénom;
            c.Nom       = vm.Nom;
            c.Email     = vm.Email;
            c.Telephone = vm.Telephone;
            c.Numéro    = vm.Numéro;
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Client modifié avec succès.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _ctx.Clients.FindAsync(id);
            if (c == null) return NotFound();
            _ctx.Clients.Remove(c);
            await _ctx.SaveChangesAsync();
            TempData["Success"] = "Client supprimé.";
            return RedirectToAction("Index");
        }
    }
}
