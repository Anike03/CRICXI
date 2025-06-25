using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string Role { get; set; } = "User";
        public decimal WalletBalance { get; set; } = 0;
        public DateTime? IsBannedUntil { get; set; }
    }
}
