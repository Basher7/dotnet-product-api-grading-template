using Domain.Entity;
using Domain.Interfaces;
using Infrastracture.Data;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Infrastracture.Repositories
{
    using Domain.Entity;
    using Domain.Interfaces;
    using Infrastracture.Data;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;

    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        public ProductRepository(MongoDbContext context, ILogger<ProductRepository> logger)
            : base(context, "Products", logger)
        {
        }

        public async Task<IEnumerable<Product>> GetByCategoryIdAsync(string categoryId)
        {
            try
            {
                return await _collection.Find(x => x.CategoryId == categoryId).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category ID: {CategoryId}", categoryId);
                throw;
            }
        }
    }
}
