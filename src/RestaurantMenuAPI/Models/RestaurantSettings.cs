using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuAPI.Models
{
    public class RestaurantSettings
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string SettingValue { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public static class SettingKeys
    {
        public const string MaxReservationsPerDay = "MAX_RESERVATIONS_PER_DAY";
        public const string MaxReservationsPerUser = "MAX_RESERVATIONS_PER_USER";
        public const string ReservationTimeSlotDuration = "RESERVATION_TIME_SLOT_DURATION";
        public const string OpeningTime = "OPENING_TIME";
        public const string ClosingTime = "CLOSING_TIME";
        public const string BookingAdvanceDays = "BOOKING_ADVANCE_DAYS";
    }
}
