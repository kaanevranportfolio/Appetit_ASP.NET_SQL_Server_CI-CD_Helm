using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuAPI.DTOs.Reservation;
using RestaurantMenuAPI.Services;
using System.Security.Claims;

namespace RestaurantMenuAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetReservations(
            [FromQuery] DateTime? date = null)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isStaffOrAdmin = User.IsInRole("Staff") || User.IsInRole("Admin");

            // Staff and Admin can see all reservations, Guests can only see their own
            var userIdFilter = isStaffOrAdmin ? null : userId;

            var reservations = await _reservationService.GetReservationsAsync(userIdFilter, date);
            return Ok(reservations);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> GetReservation(int id)
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
                return NotFound();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var isStaffOrAdmin = User.IsInRole("Staff") || User.IsInRole("Admin");

            // Check if user can access this reservation
            if (!isStaffOrAdmin && reservation.UserId != userId)
                return Forbid();

            return Ok(reservation);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreateReservationDto createReservationDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var reservation = await _reservationService.CreateReservationAsync(userId, createReservationDto);
                return CreatedAtAction(nameof(GetReservation), new { id = reservation.Id }, reservation);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<ReservationDto>> UpdateReservation(int id, [FromBody] UpdateReservationDto updateReservationDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var reservation = await _reservationService.UpdateReservationAsync(id, userId, updateReservationDto);
                if (reservation == null)
                    return NotFound();

                return Ok(reservation);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Staff,Admin")]
        public async Task<IActionResult> UpdateReservationStatus(int id, [FromBody] UpdateReservationStatusDto statusDto)
        {
            try
            {
                var result = await _reservationService.UpdateReservationStatusAsync(id, statusDto);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Reservation status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                    return Unauthorized();

                var result = await _reservationService.CancelReservationAsync(id, userId);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Reservation cancelled successfully" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("availability")]
        public async Task<ActionResult<AvailableTimeSlotsDto>> GetAvailability(
            [FromQuery] DateTime date,
            [FromQuery] int partySize = 2)
        {
            try
            {
                var availability = await _reservationService.GetAvailableTimeSlotsAsync(date, partySize);
                return Ok(availability);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
