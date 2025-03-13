using ChatBot.Dal.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyFirstEBot;

public class BotUser
{
    public long BotUserId { get; set; }
    public long TelegramUserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? PhoneNumberr { get; set; }

    public UserInfo? UserInfo { get; set; }

    public bool IsBlocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
