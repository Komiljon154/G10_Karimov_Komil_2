using ChatBot.Dal.Entites;

namespace BII.Services;

public interface IUserInfoService
{
    Task<long> AddUserInfo(UserInfo userInfo);
    Task DeleteUserInfo(long Id);
    Task<UserInfo> GetUserInfByID(long ID);
    Task UpdateUserInfo(UserInfo userInfo);
    Task<List<UserInfo>> GetAllUserInfos();
}