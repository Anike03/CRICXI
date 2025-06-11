using MongoDB.Driver;
using CRICXI.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        // ✅ REGISTER (both for normal + demo wallet)
        public async Task<bool> Register(User user, string password)
        {
            var existing = await _users.Find(u => u.Username == user.Username || u.Email == user.Email).FirstOrDefaultAsync();
            if (existing != null)
                return false;

            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            user.WalletBalance = 1000m;  // ✅ Initial $1000 demo credit
            await _users.InsertOneAsync(user);
            return true;
        }

        // ✅ For OAuth registration (without password)
        public async Task RegisterWithoutPassword(User user)
        {
            user.WalletBalance = 1000m;
            await _users.InsertOneAsync(user);
        }

        public async Task<User?> Authenticate(string username, string password)
        {
            var user = await GetByUsername(username);
            if (user == null) return null;

            if (user.IsBannedUntil.HasValue && user.IsBannedUntil.Value > DateTime.UtcNow)
            {
                return null; // banned user cannot login
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            return result == PasswordVerificationResult.Success ? user : null;
        }

        public async Task Update(User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
        }

        public async Task<List<User>> GetAllUsers()
        {
            return await _users.Find(_ => true).ToListAsync();
        }

        public async Task<bool> UpdateWallet(string userId, decimal amount, bool addFunds = true)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return false;

            if (addFunds)
                user.WalletBalance += amount;
            else
            {
                if (user.WalletBalance < amount)
                    return false;

                user.WalletBalance -= amount;
            }

            await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            return true;
        }

        public async Task<decimal> GetWalletBalance(string userId)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            return user?.WalletBalance ?? 0;
        }

        public async Task UpdateWalletByUsername(string username, decimal amount)
        {
            var user = await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user != null)
            {
                user.WalletBalance += amount;
                await _users.ReplaceOneAsync(u => u.Id == user.Id, user);
            }
        }

        public async Task<User?> GetById(string id)
        {
            return await _users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task DeleteUser(string userId)
        {
            await _users.DeleteOneAsync(u => u.Id == userId);
        }

        public async Task BanUser(string userId, int days)
        {
            var user = await GetById(userId);
            if (user == null) return;

            user.IsBannedUntil = DateTime.UtcNow.AddDays(days);
            await Update(user);
        }
    }
}
