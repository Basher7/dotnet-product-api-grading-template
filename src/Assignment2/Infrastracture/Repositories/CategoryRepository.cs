using Domain.Entity;
using Domain.Interfaces;
using Infrastracture.Data;
using Microsoft.Extensions.Logging;
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

    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        // FIX: remove invalid cast to Serilog.ILogger
        public CategoryRepository(MongoDbContext context, ILogger<CategoryRepository> logger)
            : base(context, "Categories", logger)
        {
        }
    }
}
