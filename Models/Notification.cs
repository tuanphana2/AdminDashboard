using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace AdminDashboard.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("title")]
        public string Title { get; set; } // Tiêu đề của khảo sát

        [BsonElement("message")]
        public string Description { get; set; } // Mô tả khảo sát

        [BsonElement("userMail")]
        public List<User> Questions { get; set; } = new List<User>(); // Danh sách câu hỏi

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ngày tạo

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Ngày cập nhật cuối
    }
}
