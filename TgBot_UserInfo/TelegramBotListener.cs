using BII.Services;
using ChatBot.Dal.Entites;
using MyFirstEBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace TgBot_UserInfo;

public class TelegramBotListener
{
    private static string BotToken = "7996485709:AAFviFjIEYD7cH7cNstZt9cfpo1DsMzIgRg";
    private long AdminID = 7386328037;

    private List<long> allChat = new List<long>();

    private TelegramBotClient BotClient = new TelegramBotClient(BotToken);

    private Dictionary<long, string> UserForUserInfo = new Dictionary<long, string>();

    private Dictionary<long, UserInfo> UserInfos = new Dictionary<long, UserInfo>();

    private readonly IUserService _userService;
    private readonly IUserInfoService _userInfoService;

    public TelegramBotListener(IUserInfoService userInfoService, IUserService userService)
    {
        _userInfoService = userInfoService;
        _userService = userService;
    }

    
    public string EscapeMarkdownV2(string text)
    {
        string[] specialChars = { "[", "]", "(", ")", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
        foreach (var ch in specialChars)
        {
            text = text.Replace(ch, "\\" + ch);
        }
        return text;
    }

  
    private bool ValidateFNameAndLName(string name)
    {
        foreach (var l in name)
        {
            if (!char.IsLetter(l) || l == ' ')
            {
                return false;
            }
        }
        return !string.IsNullOrEmpty(name) && name.Length <= 50;
    }

    private bool ValidatePhone(string phone)
    {
        foreach (var l in phone)
        {
            if (!char.IsDigit(l) || l == ' ')
            {
                return false;
            }
        }
        return phone.Length == 9;
    }

    private bool ValidateEmail(string email)
    {
        email.ToLower();

        return email.EndsWith("@gmail.com") && !string.IsNullOrEmpty(email) && email.Length <= 200 && email.Length > 10;
    }

    public async Task StartBot()
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message, UpdateType.InlineQuery } };

        Console.WriteLine("Your bot is starting");

        BotClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions
        );

        Console.ReadKey();
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Type == UpdateType.Message)
        {
            var message = update.Message;
            var user = message.Chat;
            BotUser userObject;
            try
            {
                userObject = await _userService.GetUserByID(user.Id);
            }
            catch (Exception ex)
            {
                userObject = null;
            }

            Console.WriteLine($"{user.Id},  {user.FirstName}, {message.Text}");
            if (message.Text == "/send")
            {
                if (userObject.TelegramUserId == AdminID)
                {
                    await bot.SendTextMessageAsync(user.Id, "Enter the word: ", cancellationToken: cancellationToken);
                    allChat.Add(AdminID);
                }
            }
            else if (allChat.Contains(AdminID))
            {
                var users = await _userService.GetAllUser();
                foreach (var u in users)
                {
                    await bot.SendTextMessageAsync(u.TelegramUserId, message.Text, cancellationToken: cancellationToken);
                }
            }

            if (message.Text == "Enter Information")
            {
                if (userObject.UserInfo is null)
                {
                    try
                    {
                        UserInfos.Add(user.Id, new UserInfo());
                        UserForUserInfo.Add(user.Id, "FirstName");
                    }
                    catch (Exception ex)
                    {
                        UserInfos.Remove(user.Id);
                        UserInfos.Add(user.Id, new UserInfo());

                        UserForUserInfo.Remove(user.Id);
                        UserForUserInfo.Add(user.Id, "FirstName");
                    }

                    await bot.SendTextMessageAsync(user.Id, "Enter your first name: ", cancellationToken: cancellationToken);
                }
                else if (userObject.UserInfo is not null)
                {
                    var userInformation = await _userInfoService.GetUserInfByID(userObject.BotUserId);
                    var userInfo = $"~You already have information~\n\n*First Name* : _{userInformation.FirstNamee}_\n" +
                        $"*Last Name* : _{userInformation.LastNamee}_\n" +
                        $"*Email* : {userInformation.Email}\n" +
                        $"*Phone Number* : {userInformation.PhoneNumber}\n" +
                        $"*Address* : `{userInformation.Address}`\n" +
                        $"*Summary* : *{userInformation.Summary}*";

                    await bot.SendTextMessageAsync(user.Id, EscapeMarkdownV2(userInfo), cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
                    return;
                }
            }
            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "FirstName")
            {
                var validate = ValidateFNameAndLName(message.Text);
                if (!validate)
                {
                    await bot.SendTextMessageAsync(user.Id, "Enter your first name correctly!!!", cancellationToken: cancellationToken);
                    return;
                }
                var info = UserInfos[user.Id];
                info.FirstNamee = message.Text;
                var ch = info.FirstNamee[0];
                info.FirstNamee = info.FirstNamee.Remove(0, 1);
                info.FirstNamee = char.ToUpper(ch) + info.FirstNamee;
                UserForUserInfo[user.Id] = "LastName";
                await bot.SendTextMessageAsync(user.Id, "Enter your last name: ", cancellationToken: cancellationToken);
            }

            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "LastName")
            {
                var validate = ValidateFNameAndLName(message.Text);
                if (!validate)
                {
                    await bot.SendTextMessageAsync(user.Id, "Enter your last name correctly!!!", cancellationToken: cancellationToken);
                    return;
                }
                var info = UserInfos[user.Id];
                info.LastNamee = message.Text;
                var ch = info.LastNamee[0];
                info.LastNamee = info.LastNamee.Remove(0, 1);
                info.LastNamee = char.ToUpper(ch) + info.LastNamee;
                UserForUserInfo[user.Id] = "Email";
                await bot.SendTextMessageAsync(user.Id, "Enter your email: ", cancellationToken: cancellationToken);
            }

            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "Email")
            {
                var validate = ValidateEmail(message.Text);
                if (!validate)
                {
                    await bot.SendTextMessageAsync(user.Id, "Enter your email correctly!!!", cancellationToken: cancellationToken);
                    return;
                }
                var info = UserInfos[user.Id];
                info.Email = message.Text;
                info.Email.ToLower();
                UserForUserInfo[user.Id] = "Phone";
                await bot.SendTextMessageAsync(user.Id, "Enter your phone number (505709110 format):", cancellationToken: cancellationToken);
            }

            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "Phone")
            {
                var validate = ValidatePhone(message.Text);
                if (!validate)
                {
                    await bot.SendTextMessageAsync(user.Id, "Enter your phone number correctly!!!", cancellationToken: cancellationToken);
                    return;
                }
                var info = UserInfos[user.Id];
                info.PhoneNumber = message.Text;
                info.PhoneNumber = "+998" + info.PhoneNumber;
                UserForUserInfo[user.Id] = "Address";
                await bot.SendTextMessageAsync(user.Id, "Enter your address: ", cancellationToken: cancellationToken);
            }

            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "Address")
            {
                if (message.Text.Length > 200 && !string.IsNullOrEmpty(message.Text))
                {
                    await bot.SendTextMessageAsync(user.Id, "Enter your address correctly!!!", cancellationToken: cancellationToken);
                    return;
                }
                var info = UserInfos[user.Id];
                info.Address = message.Text;
                UserForUserInfo[user.Id] = "Summary";
                await bot.SendTextMessageAsync(user.Id, "Enter your summary: ", cancellationToken: cancellationToken);
            }

            else if (UserForUserInfo.ContainsKey(user.Id) && UserForUserInfo[user.Id] == "Summary")
            {
                var info = UserInfos[user.Id];
                info.UserId = userObject.BotUserId;
                info.Summary = message.Text;

                await _userInfoService.AddUserInfo(info);

                UserInfos.Remove(user.Id);
                UserForUserInfo.Remove(user.Id);
                await bot.SendTextMessageAsync(user.Id, "User info saved", cancellationToken: cancellationToken);
            }

            if (message.Text == "View Information")
            {
                UserInfo userInformation;
                try
                {
                    userInformation = await _userInfoService.GetUserInfByID(userObject.BotUserId);
                }
                catch (Exception ex)
                {
                    await bot.SendTextMessageAsync(user.Id, "User info not found", cancellationToken: cancellationToken);
                    return;
                }

                var userInfo = $"*First Name* : _{userInformation.FirstNamee}_\n" +
                    $"*Last Name* : _{userInformation.LastNamee}_\n" +
                    $"*Email* : {userInformation.Email}\n" +
                    $"*Phone Number* : {userInformation.PhoneNumber}\n" +
                    $"*Address* : `{userInformation.Address}`\n" +
                    $"*Summary* : *{userInformation.Summary}*";

                await bot.SendTextMessageAsync(user.Id, EscapeMarkdownV2(userInfo), cancellationToken: cancellationToken, parseMode: ParseMode.MarkdownV2);
            }

            if (message.Text == "Delete Information")
            {
                var userInformation = await _userService.GetUserByID(user.Id);
                if (userInformation.UserInfo is null)
                {
                    await bot.SendTextMessageAsync(user.Id, "To delete information, first add information", cancellationToken: cancellationToken);
                    return;
                }
                else
                {
                    await _userInfoService.DeleteUserInfo(userInformation.UserInfo.UserId);

                    await bot.SendTextMessageAsync(user.Id, "Information deleted", cancellationToken: cancellationToken);
                }
            }

            if (message.Text == "/start")
            {
                if (userObject == null)
                {
                    userObject = new BotUser()
                    {
                        CreatedAt = DateTime.UtcNow,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        IsBlocked = false,
                        PhoneNumberr = null,
                        TelegramUserId = user.Id,
                        UpdatedAt = DateTime.UtcNow,
                        Username = user.Username
                    };

                    await _userService.AddUser(userObject);
                }
                else
                {
                    if (user.FirstName != userObject.FirstName || user.LastName != userObject.LastName || user.Username != userObject.Username)
                    {
                        userObject.UpdatedAt = DateTime.UtcNow;
                    }
                    ;
                    userObject.FirstName = user.FirstName;
                    userObject.LastName = user.LastName;
                    userObject.Username = user.Username;
                    await _userService.UpdateUser(userObject);
                }

                var keyboard = new ReplyKeyboardMarkup(new[]
                {
                                        new[]
                                        {
                                            new KeyboardButton("Enter Information"),
                                            new KeyboardButton("View Information"),
                                        },
                                        new[]
                                        {
                                            new KeyboardButton("Delete Information"),
                                        },
                   

                                    })
                { ResizeKeyboard = true };


                await bot.SendTextMessageAsync(user.Id, "Hello 👋", replyMarkup: keyboard);
                if (userObject.TelegramUserId == AdminID)
                {
                    await bot.SendTextMessageAsync(user.Id, "For send massage: /send ", replyMarkup: keyboard);
                }
                return;
            }
        }
        else if (update.Type == UpdateType.CallbackQuery)
        {
            var id = update.CallbackQuery.From.Id;

            var text = update.CallbackQuery.Data;

            CallbackQuery res = update.CallbackQuery;
        }
    }

    public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine(exception.Message);
    }
}
