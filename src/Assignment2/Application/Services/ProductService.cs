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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ICacheService _cacheService;
        private const string CacheKeyPrefix = "product:";

        public ProductService(
            IProductRepository repository,
            ICacheService cacheService)
        {
            _repository = repository;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var cacheKey = $"{CacheKeyPrefix}all";
            var cached = await _cacheService.GetAsync<IEnumerable<ProductDto>>(cacheKey);

            if (cached != null)
            {
                return cached;
            }

            var products = await _repository.GetAllAsync();
            var dtos = products.Select(MapToDto);

            await _cacheService.SetAsync(cacheKey, dtos, TimeSpan.FromMinutes(5));

            return dtos;
        }

        public async Task<ProductDto?> GetProductByIdAsync(string id)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            var cached = await _cacheService.GetAsync<ProductDto>(cacheKey);

            if (cached != null)
                return cached;

            var product = await _repository.GetByIdAsync(id);
            if (product == null)
                return null;

            var dto = MapToDto(product);
            await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10));

            return dto;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                CategoryId = dto.CategoryId
            };

            var created = await _repository.AddAsync(product);
            await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix);

            return MapToDto(created);
        }

        public async Task<bool> UpdateProductAsync(string id, UpdateProductDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
                return false;

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Price = dto.Price;
            existing.Stock = dto.Stock;
            existing.CategoryId = dto.CategoryId;
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

        public async Task<bool> DeleteProductAsync(string id)
        {
            var result = await _repository.DeleteAsync(id);
            if (result)
            {
                await _cacheService.RemoveAsync($"{CacheKeyPrefix}{id}");
                await _cacheService.RemoveByPrefixAsync(CacheKeyPrefix);
            }

            return result;
        }

        private static ProductDto MapToDto(Product product) => new()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Stock = product.Stock,
            CategoryId = product.CategoryId,
            IsActive = product.IsActive
        };
    }
}
