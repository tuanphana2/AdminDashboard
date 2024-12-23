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
        public string Title { get; set; } // Tiêu đề của thông báo

        [BsonElement("message")]
        public string Description { get; set; } // Mô tả thông báo

        [BsonElement("userMail")]
        public List<String> Emails { get; set; } = new List<String>(); // Danh sách người dùng nhận thông báo

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ngày tạo

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Ngày cập nhật cuối

        [BsonIgnoreIfDefault]
        public int __v { get; set; }
    }
}
