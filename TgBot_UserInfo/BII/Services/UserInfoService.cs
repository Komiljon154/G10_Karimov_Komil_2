using ChatBot.Dal.Entites;
using MyFirstEBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BII.Services;

public class UserInfoService : IUserInfoService
{
    private readonly MainContext _mainContext;
    public UserInfoService(MainContext maincontext)
    {
        _mainContext = maincontext;
    }
    public async Task<long> AddUserInfo(UserInfo userInfo)
    {
        _mainContext.UserInfos.Add(userInfo);
        _mainContext.SaveChanges();
        return userInfo.UserInfoId;
    }

    public async Task DeleteUserInfo(long Id)
    {
        var userInfo =await GetUserInfByID(Id);
        _mainContext.UserInfos.Remove(userInfo);
        _mainContext.SaveChanges();
    }

    public async Task<List<UserInfo>> GetAllUserInfos()
    {
        return _mainContext.UserInfos.ToList();
    }

    public async Task<UserInfo> GetUserInfByID(long ID)
    {
        var userInfo = _mainContext.UserInfos.FirstOrDefault(ui=>ui.UserId == ID);
        if (userInfo == null)
        {
            throw new Exception("UserInfo Not Found");
        }
        return userInfo;
    }

    public async Task UpdateUserInfo(UserInfo userInfo)
    {
        var oldUserInfo =await GetUserInfByID(userInfo.UserId);
        oldUserInfo.UserInfoId = userInfo.UserId;
        oldUserInfo.Address = userInfo.Address;
        oldUserInfo.PhoneNumber = userInfo.PhoneNumber;
        oldUserInfo.Summary = userInfo.Summary;
        oldUserInfo.FirstNamee = userInfo.FirstNamee;
        oldUserInfo.LastNamee = userInfo.LastNamee;
        oldUserInfo.Email = userInfo.Email;
        _mainContext.SaveChanges();
    }
}