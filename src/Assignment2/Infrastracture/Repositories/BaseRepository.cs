using Domain.Entity;
using Domain.Interfaces;
using Infrastracture.Data;
using MongoDB.Driver;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
    // FIX: use Microsoft ILogger instead of Serilog ILogger
    using Domain.Entity;
    using Domain.Interfaces;
    using Infrastracture.Data;
    using MongoDB.Driver;
    using Microsoft.Extensions.Logging;
namespace Infrastracture.Repositories
{


    public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly IMongoCollection<T> _collection;
        protected readonly ILogger _logger;

        protected BaseRepository(MongoDbContext context, string collectionName, ILogger logger)
        {
            _collection = context.GetCollection<T>(collectionName);
            _logger = logger;
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _collection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all {Type}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<T?> GetByIdAsync(string id)
        {
            try
            {
                return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting {Type} by ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                await _collection.InsertOneAsync(entity);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding {Type}", typeof(T).Name);
                throw;
            }
        }

        public virtual async Task<bool> UpdateAsync(string id, T entity)
        {
            try
            {
                entity.UpdatedAt = DateTime.UtcNow;
                var result = await _collection.ReplaceOneAsync(x => x.Id == id, entity);
                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating {Type} with ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }

        public virtual async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var result = await _collection.DeleteOneAsync(x => x.Id == id);
                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting {Type} with ID: {Id}", typeof(T).Name, id);
                throw;
            }
        }
    }
}
