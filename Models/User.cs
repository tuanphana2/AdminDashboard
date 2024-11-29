using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AdminDashboard.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = string.Empty;

        [BsonElement("image")]
        public string Image { get; set; } = string.Empty;

        [BsonElement("phone")]
        public string Phone { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("address")]
        public string Address { get; set; } = string.Empty;

        [BsonElement("hobbies")]
        public List<string> Hobbies { get; set; } = new List<string>();

        [BsonElement("role")]
        public string Role { get; set; } = "User";

        [BsonElement("activeSpeaker")]
        public bool ActiveSpeaker { get; set; } = false;

        [BsonElement("activeBlock")]
        public bool ActiveBlock { get; set; } = true;

        [BsonElement("token")]
        public List<string> Token { get; set; } = new List<string>();

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [BsonIgnoreIfDefault]
        public int __v { get; set; }
    }
}
