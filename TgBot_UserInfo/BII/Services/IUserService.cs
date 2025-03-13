using ChatBot.Dal.Entites;
using MyFirstEBot;

namespace BII.Services;

public interface IUserService
{
    Task<long> AddUser(BotUser user);
    Task DeleteUser(long Id);
    Task<BotUser> GetUserByID(long ID);
    Task UpdateUser(BotUser user);
    Task<List<BotUser>> GetAllUser();
}