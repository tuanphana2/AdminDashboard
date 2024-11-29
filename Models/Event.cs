using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace AdminDashboard.Models
{
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString(); // UUID của sự kiện, khởi tạo tự động

        [BsonElement("title")]
        public string Title { get; set; } // Tiêu đề của sự kiện

        [BsonElement("organizer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Organizer { get; set; } // UUID của người tổ chức

        [BsonElement("description")]
        public string Description { get; set; } // Mô tả sự kiện

        [BsonElement("date")]
        public DateTime Date { get; set; } // Ngày tổ chức sự kiện

        [BsonElement("location")]
        public string Location { get; set; } // Địa điểm tổ chức

        [BsonElement("price")]
        public float Price { get; set; }

        [BsonElement("image")]
        public string Image { get; set; } // URL hình ảnh sự kiện

        [BsonElement("hasLiveStream")]
        public bool HasLiveStream { get; set; } = false; // URL hình ảnh sự kiện

        [BsonElement("category_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; } // UUID của thể loại sự kiện

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ngày tạo sự kiện, mặc định là thời gian hiện tại

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Ngày cập nhật sự kiện, mặc định là thời gian hiện tại

        [BsonIgnoreIfDefault]
        public int __v { get; set; }
    }
}
