using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class ProfileHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/profile");
        
        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var database = Database.Instance();
            var user = database.GetUser(context.UserId);

            var keyboard = new InlineButtonBuilder()
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Повернутися на головну", "/start")
                );
            
            var message = $"<b>Прізвище:</b> {user.Surname}\r\n<b>Ім'я:</b> {user.Name}\r\n<b>Клас навчання:</b> {user.Form}-{user.FormLetter}";
            await client.EditMessageTextAsync(
                context.UserId,
                context.MessageId,
                message,
                ParseMode.Html,
                replyMarkup: keyboard.Build()
            );
        }
    }
}