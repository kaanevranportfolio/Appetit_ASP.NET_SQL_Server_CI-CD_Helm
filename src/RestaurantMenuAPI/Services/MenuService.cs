using Microsoft.EntityFrameworkCore;
using RestaurantMenuAPI.Data;
using RestaurantMenuAPI.DTOs.Menu;
using RestaurantMenuAPI.Models;

namespace RestaurantMenuAPI.Services
{
    public interface IMenuService
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesAsync(bool includeInactive = false);
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto);
        Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto);
        Task<bool> DeleteCategoryAsync(int id);

        Task<IEnumerable<MenuItemDto>> GetMenuItemsAsync(int? categoryId = null, bool includeInactive = false);
        Task<MenuItemDto?> GetMenuItemByIdAsync(int id);
        Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto createMenuItemDto);
        Task<MenuItemDto?> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateMenuItemDto);
        Task<bool> DeleteMenuItemAsync(int id);
        Task<bool> UpdateMenuItemAvailabilityAsync(int id, MenuItemAvailabilityDto availabilityDto);
    }

    public class MenuService : IMenuService
    {
        private readonly ApplicationDbContext _context;

        public MenuService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(bool includeInactive = false)
        {
            var query = _context.Categories.AsQueryable();

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            return await query
                .Include(c => c.MenuItems.Where(mi => includeInactive || mi.IsActive))
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    CreatedAt = c.CreatedAt,
                    MenuItems = c.MenuItems.Select(mi => new MenuItemDto
                    {
                        Id = mi.Id,
                        Name = mi.Name,
                        Description = mi.Description,
                        Price = mi.Price,
                        ImageUrl = mi.ImageUrl,
                        IsAvailable = mi.IsAvailable,
                        AvailableQuantity = mi.AvailableQuantity,
                        IsActive = mi.IsActive,
                        CategoryId = mi.CategoryId,
                        CategoryName = c.Name,
                        CreatedAt = mi.CreatedAt
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.MenuItems.Where(mi => mi.IsActive))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt,
                MenuItems = category.MenuItems.Select(mi => new MenuItemDto
                {
                    Id = mi.Id,
                    Name = mi.Name,
                    Description = mi.Description,
                    Price = mi.Price,
                    ImageUrl = mi.ImageUrl,
                    IsAvailable = mi.IsAvailable,
                    AvailableQuantity = mi.AvailableQuantity,
                    IsActive = mi.IsActive,
                    CategoryId = mi.CategoryId,
                    CategoryName = category.Name,
                    CreatedAt = mi.CreatedAt
                }).ToList()
            };
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            var category = new Category
            {
                Name = createCategoryDto.Name,
                Description = createCategoryDto.Description
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return null;

            category.Name = updateCategoryDto.Name;
            category.Description = updateCategoryDto.Description;
            category.IsActive = updateCategoryDto.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                CreatedAt = category.CreatedAt
            };
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
                return false;

            if (category.MenuItems.Any())
                throw new InvalidOperationException("Cannot delete category with existing menu items");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MenuItemDto>> GetMenuItemsAsync(int? categoryId = null, bool includeInactive = false)
        {
            var query = _context.MenuItems
                .Include(mi => mi.Category)
                .AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(mi => mi.CategoryId == categoryId.Value);

            if (!includeInactive)
                query = query.Where(mi => mi.IsActive);

            return await query
                .Select(mi => new MenuItemDto
                {
                    Id = mi.Id,
                    Name = mi.Name,
                    Description = mi.Description,
                    Price = mi.Price,
                    ImageUrl = mi.ImageUrl,
                    IsAvailable = mi.IsAvailable,
                    AvailableQuantity = mi.AvailableQuantity,
                    IsActive = mi.IsActive,
                    CategoryId = mi.CategoryId,
                    CategoryName = mi.Category.Name,
                    CreatedAt = mi.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<MenuItemDto?> GetMenuItemByIdAsync(int id)
        {
            var menuItem = await _context.MenuItems
                .Include(mi => mi.Category)
                .FirstOrDefaultAsync(mi => mi.Id == id);

            if (menuItem == null)
                return null;

            return new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                ImageUrl = menuItem.ImageUrl,
                IsAvailable = menuItem.IsAvailable,
                AvailableQuantity = menuItem.AvailableQuantity,
                IsActive = menuItem.IsActive,
                CategoryId = menuItem.CategoryId,
                CategoryName = menuItem.Category.Name,
                CreatedAt = menuItem.CreatedAt
            };
        }

        public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto createMenuItemDto)
        {
            var category = await _context.Categories.FindAsync(createMenuItemDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Invalid category ID");

            var menuItem = new MenuItem
            {
                Name = createMenuItemDto.Name,
                Description = createMenuItemDto.Description,
                Price = createMenuItemDto.Price,
                ImageUrl = createMenuItemDto.ImageUrl,
                AvailableQuantity = createMenuItemDto.AvailableQuantity,
                CategoryId = createMenuItemDto.CategoryId
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            return new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                ImageUrl = menuItem.ImageUrl,
                IsAvailable = menuItem.IsAvailable,
                AvailableQuantity = menuItem.AvailableQuantity,
                IsActive = menuItem.IsActive,
                CategoryId = menuItem.CategoryId,
                CategoryName = category.Name,
                CreatedAt = menuItem.CreatedAt
            };
        }

        public async Task<MenuItemDto?> UpdateMenuItemAsync(int id, UpdateMenuItemDto updateMenuItemDto)
        {
            var menuItem = await _context.MenuItems
                .Include(mi => mi.Category)
                .FirstOrDefaultAsync(mi => mi.Id == id);

            if (menuItem == null)
                return null;

            var category = await _context.Categories.FindAsync(updateMenuItemDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Invalid category ID");

            menuItem.Name = updateMenuItemDto.Name;
            menuItem.Description = updateMenuItemDto.Description;
            menuItem.Price = updateMenuItemDto.Price;
            menuItem.ImageUrl = updateMenuItemDto.ImageUrl;
            menuItem.IsAvailable = updateMenuItemDto.IsAvailable;
            menuItem.AvailableQuantity = updateMenuItemDto.AvailableQuantity;
            menuItem.IsActive = updateMenuItemDto.IsActive;
            menuItem.CategoryId = updateMenuItemDto.CategoryId;
            menuItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new MenuItemDto
            {
                Id = menuItem.Id,
                Name = menuItem.Name,
                Description = menuItem.Description,
                Price = menuItem.Price,
                ImageUrl = menuItem.ImageUrl,
                IsAvailable = menuItem.IsAvailable,
                AvailableQuantity = menuItem.AvailableQuantity,
                IsActive = menuItem.IsActive,
                CategoryId = menuItem.CategoryId,
                CategoryName = category.Name,
                CreatedAt = menuItem.CreatedAt
            };
        }

        public async Task<bool> DeleteMenuItemAsync(int id)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return false;

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateMenuItemAvailabilityAsync(int id, MenuItemAvailabilityDto availabilityDto)
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
                return false;

            menuItem.IsAvailable = availabilityDto.IsAvailable;
            menuItem.AvailableQuantity = availabilityDto.AvailableQuantity;
            menuItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
