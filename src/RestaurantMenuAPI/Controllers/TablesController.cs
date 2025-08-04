using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuAPI.DTOs.Reservation;
using RestaurantMenuAPI.Services;

namespace RestaurantMenuAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Staff,Admin")]
    public class TablesController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public TablesController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TableDto>>> GetTables([FromQuery] bool includeInactive = false)
        {
            var tables = await _reservationService.GetTablesAsync(includeInactive);
            return Ok(tables);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TableDto>> GetTable(int id)
        {
            var table = await _reservationService.GetTableByIdAsync(id);
            if (table == null)
                return NotFound();

            return Ok(table);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TableDto>> CreateTable([FromBody] CreateTableDto createTableDto)
        {
            try
            {
                var table = await _reservationService.CreateTableAsync(createTableDto);
                return CreatedAtAction(nameof(GetTable), new { id = table.Id }, table);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<TableDto>> UpdateTable(int id, [FromBody] UpdateTableDto updateTableDto)
        {
            try
            {
                var table = await _reservationService.UpdateTableAsync(id, updateTableDto);
                if (table == null)
                    return NotFound();

                return Ok(table);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTable(int id)
        {
            try
            {
                var result = await _reservationService.DeleteTableAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
