using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AdminDashboard.Models
{
    public class CategoryEvent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // Tạo Id dạng ObjectId tự động

        [BsonElement("name")]
        public string Name { get; set; } // Tên của danh mục, NOT NULL

        [BsonElement("description")]
        public string Description { get; set; } // Mô tả của danh mục

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ngày tạo

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Ngày cập nhật cuối

        [BsonIgnoreIfDefault]
        public int __v { get; set; }
    }
}
