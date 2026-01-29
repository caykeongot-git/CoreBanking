using CoreBanking.BLL.Interfaces;
using CoreBanking.DAL.Entities;
using CoreBanking.DAL.Repositories;

namespace CoreBanking.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            // Tìm user có username trùng khớp
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username && !u.IsDeleted);
            var user = users.FirstOrDefault();

            if (user == null) return null;

            // So sánh Password (Ở đây so sánh thô, thực tế cần so sánh Hash)
            if (user.PasswordHash == password)
            {
                return user;
            }

            return null;
        }
    }
}