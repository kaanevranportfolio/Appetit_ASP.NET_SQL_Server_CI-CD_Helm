using Microsoft.EntityFrameworkCore;
using RestaurantMenuAPI.Data;
using RestaurantMenuAPI.DTOs.Reservation;
using RestaurantMenuAPI.Models;

namespace RestaurantMenuAPI.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDto>> GetReservationsAsync(string? userId = null, DateTime? date = null);
        Task<ReservationDto?> GetReservationByIdAsync(int id);
        Task<ReservationDto> CreateReservationAsync(string userId, CreateReservationDto createReservationDto);
        Task<ReservationDto?> UpdateReservationAsync(int id, string userId, UpdateReservationDto updateReservationDto);
        Task<bool> UpdateReservationStatusAsync(int id, UpdateReservationStatusDto statusDto);
        Task<bool> CancelReservationAsync(int id, string userId);
        Task<AvailableTimeSlotsDto> GetAvailableTimeSlotsAsync(DateTime date, int partySize);
        Task<bool> ValidateReservationLimitsAsync(string userId, DateTime date);

        Task<IEnumerable<TableDto>> GetTablesAsync(bool includeInactive = false);
        Task<TableDto?> GetTableByIdAsync(int id);
        Task<TableDto> CreateTableAsync(CreateTableDto createTableDto);
        Task<TableDto?> UpdateTableAsync(int id, UpdateTableDto updateTableDto);
        Task<bool> DeleteTableAsync(int id);
    }

    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;

        public ReservationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReservationDto>> GetReservationsAsync(string? userId = null, DateTime? date = null)
        {
            var query = _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(r => r.UserId == userId);

            if (date.HasValue)
                query = query.Where(r => r.ReservationDate.Date == date.Value.Date);

            return await query
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.ReservationTime)
                .Select(r => new ReservationDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    TableId = r.TableId,
                    TableNumber = r.Table.TableNumber,
                    ReservationDate = r.ReservationDate,
                    ReservationTime = r.ReservationTime,
                    PartySize = r.PartySize,
                    SpecialRequests = r.SpecialRequests,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ReservationDto?> GetReservationByIdAsync(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return null;

            return new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = $"{reservation.User.FirstName} {reservation.User.LastName}",
                TableId = reservation.TableId,
                TableNumber = reservation.Table.TableNumber,
                ReservationDate = reservation.ReservationDate,
                ReservationTime = reservation.ReservationTime,
                PartySize = reservation.PartySize,
                SpecialRequests = reservation.SpecialRequests,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };
        }

        public async Task<ReservationDto> CreateReservationAsync(string userId, CreateReservationDto createReservationDto)
        {
            // Validate user limits
            var canMakeReservation = await ValidateReservationLimitsAsync(userId, createReservationDto.ReservationDate);
            if (!canMakeReservation)
                throw new InvalidOperationException("Reservation limits exceeded");

            // Validate table availability
            var isTableAvailable = await IsTableAvailableAsync(
                createReservationDto.TableId,
                createReservationDto.ReservationDate,
                createReservationDto.ReservationTime);

            if (!isTableAvailable)
                throw new InvalidOperationException("Table is not available at the requested time");

            // Validate table capacity
            var table = await _context.Tables.FindAsync(createReservationDto.TableId);
            if (table == null || !table.IsActive)
                throw new ArgumentException("Invalid table ID");

            if (createReservationDto.PartySize > table.Capacity)
                throw new InvalidOperationException($"Party size exceeds table capacity of {table.Capacity}");

            var reservation = new Reservation
            {
                UserId = userId,
                TableId = createReservationDto.TableId,
                ReservationDate = createReservationDto.ReservationDate,
                ReservationTime = createReservationDto.ReservationTime,
                PartySize = createReservationDto.PartySize,
                SpecialRequests = createReservationDto.SpecialRequests
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = $"{user!.FirstName} {user.LastName}",
                TableId = reservation.TableId,
                TableNumber = table.TableNumber,
                ReservationDate = reservation.ReservationDate,
                ReservationTime = reservation.ReservationTime,
                PartySize = reservation.PartySize,
                SpecialRequests = reservation.SpecialRequests,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };
        }

        public async Task<ReservationDto?> UpdateReservationAsync(int id, string userId, UpdateReservationDto updateReservationDto)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Table)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reservation == null)
                return null;

            // Only allow the user who made the reservation to update it (or admin)
            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException("You can only update your own reservations");

            // Validate table availability if table or time changes
            if (reservation.TableId != updateReservationDto.TableId ||
                reservation.ReservationDate != updateReservationDto.ReservationDate ||
                reservation.ReservationTime != updateReservationDto.ReservationTime)
            {
                var isTableAvailable = await IsTableAvailableAsync(
                    updateReservationDto.TableId,
                    updateReservationDto.ReservationDate,
                    updateReservationDto.ReservationTime,
                    id);

                if (!isTableAvailable)
                    throw new InvalidOperationException("Table is not available at the requested time");
            }

            // Validate table capacity
            var table = await _context.Tables.FindAsync(updateReservationDto.TableId);
            if (table == null || !table.IsActive)
                throw new ArgumentException("Invalid table ID");

            if (updateReservationDto.PartySize > table.Capacity)
                throw new InvalidOperationException($"Party size exceeds table capacity of {table.Capacity}");

            reservation.TableId = updateReservationDto.TableId;
            reservation.ReservationDate = updateReservationDto.ReservationDate;
            reservation.ReservationTime = updateReservationDto.ReservationTime;
            reservation.PartySize = updateReservationDto.PartySize;
            reservation.SpecialRequests = updateReservationDto.SpecialRequests;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ReservationDto
            {
                Id = reservation.Id,
                UserId = reservation.UserId,
                UserName = $"{reservation.User.FirstName} {reservation.User.LastName}",
                TableId = reservation.TableId,
                TableNumber = table.TableNumber,
                ReservationDate = reservation.ReservationDate,
                ReservationTime = reservation.ReservationTime,
                PartySize = reservation.PartySize,
                SpecialRequests = reservation.SpecialRequests,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt
            };
        }

        public async Task<bool> UpdateReservationStatusAsync(int id, UpdateReservationStatusDto statusDto)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return false;

            reservation.Status = statusDto.Status;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelReservationAsync(int id, string userId)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
                return false;

            if (reservation.UserId != userId)
                throw new UnauthorizedAccessException("You can only cancel your own reservations");

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AvailableTimeSlotsDto> GetAvailableTimeSlotsAsync(DateTime date, int partySize)
        {
            var openingTime = TimeSpan.Parse(await GetSettingValueAsync(SettingKeys.OpeningTime) ?? "11:00");
            var closingTime = TimeSpan.Parse(await GetSettingValueAsync(SettingKeys.ClosingTime) ?? "22:00");
            var slotDuration = int.Parse(await GetSettingValueAsync(SettingKeys.ReservationTimeSlotDuration) ?? "120");

            var timeSlots = new List<TimeSlotDto>();
            var currentTime = openingTime;

            while (currentTime.Add(TimeSpan.FromMinutes(slotDuration)) <= closingTime)
            {
                var availableTables = await GetAvailableTablesAsync(date, currentTime, partySize);
                
                timeSlots.Add(new TimeSlotDto
                {
                    Time = currentTime,
                    IsAvailable = availableTables.Any(),
                    AvailableTableIds = availableTables.Select(t => t.Id).ToList()
                });

                currentTime = currentTime.Add(TimeSpan.FromMinutes(30)); // 30-minute intervals
            }

            return new AvailableTimeSlotsDto
            {
                Date = date,
                TimeSlots = timeSlots
            };
        }

        public async Task<bool> ValidateReservationLimitsAsync(string userId, DateTime date)
        {
            var maxReservationsPerUser = int.Parse(await GetSettingValueAsync(SettingKeys.MaxReservationsPerUser) ?? "3");
            var maxReservationsPerDay = int.Parse(await GetSettingValueAsync(SettingKeys.MaxReservationsPerDay) ?? "50");

            // Check user's active reservations
            var userActiveReservations = await _context.Reservations
                .CountAsync(r => r.UserId == userId && 
                           r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed);

            if (userActiveReservations >= maxReservationsPerUser)
                return false;

            // Check daily reservations
            var dailyReservations = await _context.Reservations
                .CountAsync(r => r.ReservationDate.Date == date.Date && 
                           (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed));

            return dailyReservations < maxReservationsPerDay;
        }

        // Table management methods
        public async Task<IEnumerable<TableDto>> GetTablesAsync(bool includeInactive = false)
        {
            var query = _context.Tables.AsQueryable();

            if (!includeInactive)
                query = query.Where(t => t.IsActive);

            return await query
                .OrderBy(t => t.TableNumber)
                .Select(t => new TableDto
                {
                    Id = t.Id,
                    TableNumber = t.TableNumber,
                    Capacity = t.Capacity,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<TableDto?> GetTableByIdAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return null;

            return new TableDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                IsActive = table.IsActive,
                CreatedAt = table.CreatedAt
            };
        }

        public async Task<TableDto> CreateTableAsync(CreateTableDto createTableDto)
        {
            var existingTable = await _context.Tables
                .FirstOrDefaultAsync(t => t.TableNumber == createTableDto.TableNumber);

            if (existingTable != null)
                throw new InvalidOperationException("Table number already exists");

            var table = new Table
            {
                TableNumber = createTableDto.TableNumber,
                Capacity = createTableDto.Capacity
            };

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            return new TableDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                IsActive = table.IsActive,
                CreatedAt = table.CreatedAt
            };
        }

        public async Task<TableDto?> UpdateTableAsync(int id, UpdateTableDto updateTableDto)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return null;

            var existingTable = await _context.Tables
                .FirstOrDefaultAsync(t => t.TableNumber == updateTableDto.TableNumber && t.Id != id);

            if (existingTable != null)
                throw new InvalidOperationException("Table number already exists");

            table.TableNumber = updateTableDto.TableNumber;
            table.Capacity = updateTableDto.Capacity;
            table.IsActive = updateTableDto.IsActive;
            table.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new TableDto
            {
                Id = table.Id,
                TableNumber = table.TableNumber,
                Capacity = table.Capacity,
                IsActive = table.IsActive,
                CreatedAt = table.CreatedAt
            };
        }

        public async Task<bool> DeleteTableAsync(int id)
        {
            var table = await _context.Tables.FindAsync(id);
            if (table == null)
                return false;

            var hasReservations = await _context.Reservations
                .AnyAsync(r => r.TableId == id);

            if (hasReservations)
                throw new InvalidOperationException("Cannot delete table with existing reservations");

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();
            return true;
        }

        // Private helper methods
        private async Task<bool> IsTableAvailableAsync(int tableId, DateTime date, TimeSpan time, int? excludeReservationId = null)
        {
            var slotDuration = int.Parse(await GetSettingValueAsync(SettingKeys.ReservationTimeSlotDuration) ?? "120");
            var endTime = time.Add(TimeSpan.FromMinutes(slotDuration));

            var conflictingReservations = await _context.Reservations
                .Where(r => r.TableId == tableId &&
                           r.ReservationDate.Date == date.Date &&
                           (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Confirmed) &&
                           (excludeReservationId == null || r.Id != excludeReservationId) &&
                           ((r.ReservationTime < endTime) && (r.ReservationTime.Add(TimeSpan.FromMinutes(slotDuration)) > time)))
                .AnyAsync();

            return !conflictingReservations;
        }

        private async Task<List<Table>> GetAvailableTablesAsync(DateTime date, TimeSpan time, int partySize)
        {
            var tables = await _context.Tables
                .Where(t => t.IsActive && t.Capacity >= partySize)
                .ToListAsync();

            var availableTables = new List<Table>();

            foreach (var table in tables)
            {
                var isAvailable = await IsTableAvailableAsync(table.Id, date, time);
                if (isAvailable)
                    availableTables.Add(table);
            }

            return availableTables;
        }

        private async Task<string?> GetSettingValueAsync(string key)
        {
            var setting = await _context.RestaurantSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key);
            return setting?.SettingValue;
        }
    }
}
