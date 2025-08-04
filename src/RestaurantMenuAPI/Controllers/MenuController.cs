using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantMenuAPI.DTOs.Menu;
using RestaurantMenuAPI.Services;

namespace RestaurantMenuAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;

        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }

        // Categories endpoints
        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories([FromQuery] bool includeInactive = false)
        {
            var categories = await _menuService.GetCategoriesAsync(includeInactive);
            return Ok(categories);
        }

        [HttpGet("categories/{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _menuService.GetCategoryByIdAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost("categories")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = await _menuService.CreateCategoryAsync(createCategoryDto);
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                var category = await _menuService.UpdateCategoryAsync(id, updateCategoryDto);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _menuService.DeleteCategoryAsync(id);
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

        // Menu Items endpoints
        [HttpGet("items")]
        public async Task<ActionResult<IEnumerable<MenuItemDto>>> GetMenuItems(
            [FromQuery] int? categoryId = null,
            [FromQuery] bool includeInactive = false)
        {
            var menuItems = await _menuService.GetMenuItemsAsync(categoryId, includeInactive);
            return Ok(menuItems);
        }

        [HttpGet("items/{id}")]
        public async Task<ActionResult<MenuItemDto>> GetMenuItem(int id)
        {
            var menuItem = await _menuService.GetMenuItemByIdAsync(id);
            if (menuItem == null)
                return NotFound();

            return Ok(menuItem);
        }

        [HttpPost("items")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<MenuItemDto>> CreateMenuItem([FromBody] CreateMenuItemDto createMenuItemDto)
        {
            try
            {
                var menuItem = await _menuService.CreateMenuItemAsync(createMenuItemDto);
                return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, menuItem);
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

        [HttpPut("items/{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<MenuItemDto>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemDto updateMenuItemDto)
        {
            try
            {
                var menuItem = await _menuService.UpdateMenuItemAsync(id, updateMenuItemDto);
                if (menuItem == null)
                    return NotFound();

                return Ok(menuItem);
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

        [HttpDelete("items/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            try
            {
                var result = await _menuService.DeleteMenuItemAsync(id);
                if (!result)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("items/{id}/availability")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateMenuItemAvailability(int id, [FromBody] MenuItemAvailabilityDto availabilityDto)
        {
            try
            {
                var result = await _menuService.UpdateMenuItemAvailabilityAsync(id, availabilityDto);
                if (!result)
                    return NotFound();

                return Ok(new { message = "Menu item availability updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
