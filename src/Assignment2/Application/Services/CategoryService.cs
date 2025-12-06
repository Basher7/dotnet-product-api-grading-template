using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entity;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "category:";

        public CategoryService(
            ICategoryRepository repository,
            ICacheService cacheService)
        {
            _repository = repository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var cacheKey = $"{CacheKeyPrefix}all";
            var cached = await _cacheService.GetAsync<IEnumerable<CategoryDto>>(cacheKey);

            if (cached != null)
            {
                return cached;
            }

            var categories = await _repository.GetAllAsync();
            var dtos = categories.Select(MapToDto);

            await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5));

            return dtos;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(string id)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            var cached = await _cacheService.GetAsync<CategoryDto>(cacheKey);

            if (cached != null)
                return cached;

            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                return null;

            var dto = MapToDto(category);
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

            return dto;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Name = dto.Name,
                Description = dto.Description
            };

            var created = await _repository.AddAsync(category);
            await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix);

            return MapToDto(created);
        }

        public async Task<bool> UpdateCategoryAsync(string id, UpdateCategoryDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.IsActive = dto.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            var result = await _repository.UpdateAsync(id, existing);
            if (result)
            {
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
                await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix);
            }

            return result;
        }

        public async Task<bool> DeleteCategoryAsync(string id)
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
            {
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
                await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix);
            }

            return result;
        }

        private static CategoryDto MapToDto(Category category) => new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }
}

