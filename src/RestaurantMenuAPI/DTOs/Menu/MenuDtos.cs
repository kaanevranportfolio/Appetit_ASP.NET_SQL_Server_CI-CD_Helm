using System.ComponentModel.DataAnnotations;

namespace RestaurantMenuAPI.DTOs.Menu
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MenuItemDto> MenuItems { get; set; } = new List<MenuItemDto>();
    }

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class MenuItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public int? AvailableQuantity { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateMenuItemDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public int? AvailableQuantity { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateMenuItemDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;
        public int? AvailableQuantity { get; set; }
        public bool IsActive { get; set; } = true;

        [Required]
        public int CategoryId { get; set; }
    }

    public class MenuItemAvailabilityDto
    {
        public bool IsAvailable { get; set; }
        public int? AvailableQuantity { get; set; }
    }
}
