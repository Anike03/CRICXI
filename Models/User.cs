using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CRICXI.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public decimal WalletBalance { get; set; } = 0;
        public string Role { get; set; } = "User";
        public DateTime? IsBannedUntil { get; set; }

        // ✅ Add this line for Firebase integration
        public string FirebaseUid { get; set; } = string.Empty;

        public bool IsEmailConfirmed { get; set; } = true;
    }

}
