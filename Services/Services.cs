using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using TunisiaStay.Data;
using TunisiaStay.Models;
using TunisiaStay.ViewModels;

namespace TunisiaStay.Services
{
    // ═══════════════════════════════════════════════════════════════════
    //  REPOSITORY PATTERN
    // ═══════════════════════════════════════════════════════════════════
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<int> SaveAsync();
    }

    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();
        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);
        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);
        public void Update(T entity) => _dbSet.Update(entity);
        public void Delete(T entity) => _dbSet.Remove(entity);
        public async Task<int> SaveAsync() => await _context.SaveChangesAsync();
    }

    // ── Unit of Work ──────────────────────────────────────────────────
    public interface IUnitOfWork : IDisposable
    {
        IHotelRepository Hotels { get; }
        IChambreRepository Chambres { get; }
        IReservationRepository Reservations { get; }
        IClientRepository Clients { get; }
        Task<int> CompleteAsync();
    }

    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IHotelRepository Hotels { get; }
        public IChambreRepository Chambres { get; }
        public IReservationRepository Reservations { get; }
        public IClientRepository Clients { get; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Hotels = new HotelRepository(context);
            Chambres = new ChambreRepository(context);
            Reservations = new ReservationRepository(context);
            Clients = new ClientRepository(context);
        }

        public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
        public void Dispose() => _context.Dispose();
    }

    // ══ Hotel Repository ═══════════════════════════════════════════════
    public interface IHotelRepository : IRepository<Hotel>
    {
        Task<IEnumerable<Hotel>> GetHotelsWithChambresAsync();
        Task<Hotel?> GetHotelDetailAsync(int id);
        Task<IEnumerable<string>> GetCitiesAsync();
    }

    public class HotelRepository : Repository<Hotel>, IHotelRepository
    {
        public HotelRepository(ApplicationDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Hotel>> GetHotelsWithChambresAsync()
            => await _context.Hotels
                .Include(h => h.Chambres)
                .Include(h => h.Avis)
                .Where(h => h.IsActive)
                .ToListAsync();

        public async Task<Hotel?> GetHotelDetailAsync(int id)
            => await _context.Hotels
                .Include(h => h.Chambres)
                    .ThenInclude(c => c.ChambreAmenites)
                    .ThenInclude(ca => ca.Amenite)
                .Include(h => h.Avis)
                    .ThenInclude(a => a.ApplicationUser)
                .FirstOrDefaultAsync(h => h.Id == id);

        public async Task<IEnumerable<string>> GetCitiesAsync()
            => await _context.Hotels.Select(h => h.City).Distinct().ToListAsync();
    }

    // ══ Chambre Repository ════════════════════════════════════════════
    public interface IChambreRepository : IRepository<Chambre>
    {
        Task<IEnumerable<Chambre>> SearchAsync(string? city, string? model,
            decimal? min, decimal? max, int? cap, DateTime? arrival, DateTime? depart);
        Task<Chambre?> GetChambreDetailAsync(int id);
    }

    public class ChambreRepository : Repository<Chambre>, IChambreRepository
    {
        public ChambreRepository(ApplicationDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Chambre>> SearchAsync(string? city, string? model,
            decimal? min, decimal? max, int? cap, DateTime? arrival, DateTime? depart)
        {
            var query = _context.Chambres
                .Include(c => c.Hotel)
                .Include(c => c.ChambreAmenites)
                    .ThenInclude(ca => ca.Amenite)
                .Where(c => c.Disponible && c.Hotel.IsActive);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(c => c.Hotel.City == city);
            if (!string.IsNullOrWhiteSpace(model))
                query = query.Where(c => c.Model.ToLower().Contains(model.ToLower()));
            if (min.HasValue)
                query = query.Where(c => c.PrixParNuit >= min.Value);
            if (max.HasValue)
                query = query.Where(c => c.PrixParNuit <= max.Value);
            if (cap.HasValue)
                query = query.Where(c => c.Capacite >= cap.Value);

            if (arrival.HasValue && depart.HasValue)
            {
                var bookedIds = await _context.Reservations
                    .Where(r => r.Statut != "Annulée" &&
                                r.Date < depart.Value &&
                                r.Date.AddDays(r.Durée) > arrival.Value)
                    .Select(r => r.ChambreId)
                    .ToListAsync();
                query = query.Where(c => !bookedIds.Contains(c.IdChambre));
            }

            return await query.ToListAsync();
        }

        public async Task<Chambre?> GetChambreDetailAsync(int id)
            => await _context.Chambres
                .Include(c => c.Hotel)
                .Include(c => c.ChambreAmenites)
                    .ThenInclude(ca => ca.Amenite)
                .FirstOrDefaultAsync(c => c.IdChambre == id);
    }

    // ══ Reservation Repository ════════════════════════════════════════
    public interface IReservationRepository : IRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetByClientAsync(int clientId);
        Task<IEnumerable<Reservation>> GetAllWithDetailsAsync();
        Task<Reservation?> GetDetailAsync(int id);
    }

    public class ReservationRepository : Repository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext ctx) : base(ctx) { }

        public async Task<IEnumerable<Reservation>> GetByClientAsync(int clientId)
            => await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .Include(r => r.Paiement)
                .Where(r => r.ClientId == clientId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Reservation>> GetAllWithDetailsAsync()
            => await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .Include(r => r.Client)
                .Include(r => r.Paiement)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<Reservation?> GetDetailAsync(int id)
            => await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .Include(r => r.Client)
                .Include(r => r.Paiement)
                .FirstOrDefaultAsync(r => r.Id == id);
    }

    // ══ Client Repository ═════════════════════════════════════════════
    public interface IClientRepository : IRepository<Client>
    {
        Task<Client?> GetByUserIdAsync(string userId);
    }

    public class ClientRepository : Repository<Client>, IClientRepository
    {
        public ClientRepository(ApplicationDbContext ctx) : base(ctx) { }

        public async Task<Client?> GetByUserIdAsync(string userId)
            => await _context.Clients
                .Include(c => c.Reservations)
                .FirstOrDefaultAsync(c => c.ApplicationUserId == userId);
    }

    // ══ Statistics Service ════════════════════════════════════════════
    public interface IStatisticsService
    {
        Task<StatisticsViewModel> GetStatisticsAsync();
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;
        public StatisticsService(ApplicationDbContext context) => _context = context;

        public async Task<StatisticsViewModel> GetStatisticsAsync()
        {
            var vm = new StatisticsViewModel
            {
                TotalHotels = await _context.Hotels.CountAsync(h => h.IsActive),
                TotalChambres = await _context.Chambres.CountAsync(),
                TotalClients = await _context.Clients.CountAsync(),
                TotalReservations = await _context.Reservations.CountAsync(),
                RevenuTotal = (decimal)await _context.Reservations
                    .Where(r => r.Statut == "Confirmée")
                    .SumAsync(r => r.Prix),
                ReservationsEnAttente = await _context.Reservations.CountAsync(r => r.Statut == "En attente"),
                ReservationsConfirmees = await _context.Reservations.CountAsync(r => r.Statut == "Confirmée"),
                ReservationsAnnulees = await _context.Reservations.CountAsync(r => r.Statut == "Annulée"),
                TopHotels = await _context.Hotels
                    .Include(h => h.Chambres)
                    .OrderByDescending(h => h.Chambres.Count)
                    .Take(5)
                    .ToListAsync()
            };

            var byCity = await _context.Reservations
                .Include(r => r.Chambre).ThenInclude(c => c.Hotel)
                .GroupBy(r => r.Chambre.Hotel.City)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .ToListAsync();

            vm.ReservationsParVille = byCity.Select(x => (x.City, x.Count)).ToList();

            return vm;
        }
    }

    // ══ Image Service (sans dépendance externe) ═══════════════════════
    public interface IImageService
    {
        Task<string?> SaveImageAsync(IFormFile file, string folder);
        void DeleteImage(string? path);
    }

    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        public ImageService(IWebHostEnvironment env) => _env = env;

        public async Task<string?> SaveImageAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0) return null;
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return null;

            var uploads = Path.Combine(_env.WebRootPath, "images", folder);
            Directory.CreateDirectory(uploads);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/images/{folder}/{fileName}";
        }

        public void DeleteImage(string? path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var full = Path.Combine(_env.WebRootPath, path.TrimStart('/'));
            if (File.Exists(full)) File.Delete(full);
        }
    }
}
