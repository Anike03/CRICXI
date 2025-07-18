using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace CRICXI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; } = 1000; // Default 1000 for new users
        public string Role { get; set; } = "User";
        public DateTime? IsBannedUntil { get; set; }
        public string FirebaseUid { get; set; } = string.Empty;
        public bool IsEmailConfirmed { get; set; } = true;

        // Add transaction history
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public class Transaction
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}