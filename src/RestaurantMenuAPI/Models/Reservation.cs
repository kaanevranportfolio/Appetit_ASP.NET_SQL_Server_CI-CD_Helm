using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuAPI.Models
{
    public class Table
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string TableNumber { get; set; } = string.Empty;

        [Required]
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }

    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int TableId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan ReservationTime { get; set; }

        [Required]
        public int PartySize { get; set; }

        [MaxLength(500)]
        public string? SpecialRequests { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Table Table { get; set; } = null!;
        public Order? Order { get; set; }
    }

    public enum ReservationStatus
    {
        Pending = 1,
        Confirmed = 2,
        Cancelled = 3,
        Completed = 4,
        NoShow = 5
    }
}
