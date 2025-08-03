using RestaurantMenuAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuAPI.DTOs.Reservation
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public int TableId { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; }
        public TimeSpan ReservationTime { get; set; }
        public int PartySize { get; set; }
        public string? SpecialRequests { get; set; }
        public ReservationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateReservationDto
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan ReservationTime { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Party size must be between 1 and 20")]
        public int PartySize { get; set; }

        [MaxLength(500)]
        public string? SpecialRequests { get; set; }
    }

    public class UpdateReservationDto
    {
        [Required]
        public int TableId { get; set; }

        [Required]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan ReservationTime { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Party size must be between 1 and 20")]
        public int PartySize { get; set; }

        [MaxLength(500)]
        public string? SpecialRequests { get; set; }
    }

    public class UpdateReservationStatusDto
    {
        [Required]
        public ReservationStatus Status { get; set; }
    }

    public class TableDto
    {
        public int Id { get; set; }
        public string TableNumber { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTableDto
    {
        [Required]
        [MaxLength(20)]
        public string TableNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
        public int Capacity { get; set; }
    }

    public class UpdateTableDto
    {
        [Required]
        [MaxLength(20)]
        public string TableNumber { get; set; } = string.Empty;

        [Required]
        [Range(1, 20, ErrorMessage = "Capacity must be between 1 and 20")]
        public int Capacity { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class AvailableTimeSlotsDto
    {
        public DateTime Date { get; set; }
        public List<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
    }

    public class TimeSlotDto
    {
        public TimeSpan Time { get; set; }
        public bool IsAvailable { get; set; }
        public List<int> AvailableTableIds { get; set; } = new List<int>();
    }
}
