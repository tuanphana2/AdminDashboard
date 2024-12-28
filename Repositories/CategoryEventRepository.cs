using AdminDashboard.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminDashboard.Repositories
{
    public class CategoryEventRepository
    {
        private readonly IMongoCollection<CategoryEvent> _categories;

        public CategoryEventRepository(IMongoDatabase database)
        {
            _categories = database.GetCollection<CategoryEvent>("categoryevents");
        }

        public async Task<List<CategoryEvent>> GetAllAsync()
        {
            return await _categories.Find(_ => true).ToListAsync();
        }

        public async Task<(List<CategoryEvent> categories, int total)> GetCategoriesByPageAsync(int page, int itemsPerPage, string searchQuery)
        {
            var filter = Builders<CategoryEvent>.Filter.Empty;

            if (!string.IsNullOrEmpty(searchQuery))
            {
                filter = Builders<CategoryEvent>.Filter.Regex("Name", new MongoDB.Bson.BsonRegularExpression(searchQuery, "i"));
            }

            var totalCategories = await _categories.CountDocumentsAsync(filter);

            var categories = await _categories.Find(filter)
                                               .Skip((page - 1) * itemsPerPage) 
                                               .Limit(itemsPerPage)     
                                               .ToListAsync();

            return (categories, (int)totalCategories);
        }

        public async Task<CategoryEvent> GetByIdAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(CategoryEvent category)
        {
            if (string.IsNullOrEmpty(category.Id))
            {
                category.Id = ObjectId.GenerateNewId().ToString();
            }

            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;
            await _categories.InsertOneAsync(category);
        }

        public async Task UpdateAsync(string id, CategoryEvent category)
        {
            if (category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            category.UpdatedAt = DateTime.UtcNow;

            var result = await _categories.ReplaceOneAsync(c => c.Id == id, category);
            if (result.MatchedCount == 0)
            {
                throw new KeyNotFoundException("CategoryEvent not found for the provided ID.");
            }
        }

        public async Task DeleteAsync(string id)
        {
            var result = await _categories.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                throw new KeyNotFoundException("CategoryEvent not found for the provided ID.");
            }
        }

        public async Task<Dictionary<string, int>> GetEventCountByCategoryAsync(IMongoCollection<Event> eventsCollection)
        {
            var categoryEventCounts = new Dictionary<string, int>();
            var categories = await _categories.Find(_ => true).ToListAsync();

            foreach (var category in categories)
            {
                if (ObjectId.TryParse(category.Id, out ObjectId categoryIdObject))
                {
                    int eventCount = (int)await eventsCollection.CountDocumentsAsync(e => e.CategoryId == categoryIdObject.ToString());
                    categoryEventCounts[category.Name] = eventCount;
                }
            }

            return categoryEventCounts;
        }
    }
}
