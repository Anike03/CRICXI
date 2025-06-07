using MongoDB.Driver;
using CRICXI.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace CRICXI.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;
        private readonly PasswordHasher<User> _passwordHasher;

        public UserService(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("Users");
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<User?> GetByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<User?> GetByVerificationToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            Console.WriteLine($"🔍 Searching for token: {token}");
            return await _users.Find(u => u.EmailVerificationToken == token).FirstOrDefaultAsync();
        }

        public async Task<bool> Register(User user, string password)
        {
            var existing = await _users.Find(u => u.Username == user.Username || u.Email == user.Email).FirstOrDefaultAsync();
            if (existing != null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _users.InsertOneAsync(user);
            return true;
        }

        public async Task<User?> Authenticate(string username, string password)
        {
            var user = await GetByUsername(username);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        public async Task Update(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }
    }
}
