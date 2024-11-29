﻿using AdminDashboard.Models;
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

        // Lấy tất cả các danh mục sự kiện
        public async Task<List<CategoryEvent>> GetAllAsync()
        {
            return await _categories.Find(_ => true).ToListAsync();
        }

        // Lấy một danh mục sự kiện theo ID
        public async Task<CategoryEvent> GetByIdAsync(string id)
        {
            return await _categories.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        // Tạo một danh mục sự kiện mới
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

        // Cập nhật một danh mục sự kiện theo ID
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

        // Xóa một danh mục sự kiện theo ID
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
                // Chuyển đổi category.Id từ string sang ObjectId để so sánh
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
