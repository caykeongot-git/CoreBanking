using CoreBanking.DAL.Entities;

namespace CoreBanking.BLL.Interfaces
{
    public interface IUserService
    {
        Task<User?> LoginAsync(string username, string password);
    }
}