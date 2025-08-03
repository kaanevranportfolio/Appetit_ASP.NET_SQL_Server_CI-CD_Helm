using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public int? ReservationId { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Reservation? Reservation { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }

        [MaxLength(200)]
        public string? SpecialInstructions { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public MenuItem MenuItem { get; set; } = null!;
    }

    public enum OrderStatus
    {
        Pending = 1,
        Confirmed = 2,
        Preparing = 3,
        Ready = 4,
        Served = 5,
        Completed = 6,
        Cancelled = 7
    }
}
