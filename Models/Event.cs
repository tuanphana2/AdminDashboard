using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using AdminDashboard.Attributes;

namespace AdminDashboard.Models
{
    public class Event
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("organizer_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Organizer { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("date")]
        [Required(ErrorMessage = "Event date is required.")]
        [FutureDate(ErrorMessage = "The event date must be today or in the future.")]
        public DateTime Date { get; set; }

        [BsonElement("location")]
        public string Location { get; set; }

        [BsonElement("images")]
        public string Image { get; set; } = string.Empty;

        [BsonElement("category_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoryId { get; set; }

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonIgnoreIfDefault]
        public int __v { get; set; }
    }
}
