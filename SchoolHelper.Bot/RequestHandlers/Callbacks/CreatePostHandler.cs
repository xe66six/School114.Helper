using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class CreatePostHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/create");

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var database = Database.Instance();
            var user = database.GetUser(context.UserId);

            var keyboard = new InlineButtonBuilder()
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Повернутися на головну", "/start")
                );
            
            if (user.IsBanned)
            {
                await client.EditMessageTextAsync(
                    context.UserId,
                    context.MessageId,
                    "<b>🚫 Ви заблоковані та не маєте права створювати публікації</b>",
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboard.Build()
                );
                return;
            }
            
            database.AddToQueue(context.UserId, "new_post", context.MessageId);
            
            var message = 
                "<b>Створення ідеї</b>\r\n\r\nЩоб створити ідею, напишіть текст в повідомленні та очікуйте підтвердження публікації зі сторони модерації.\r\n\r\nЗверніть увагу, що контент кожного повідомлення модеруєтся особисто. Якщо будуть знайдені порушення (образливий, негативний, безглуздий, або дискримінаціний характер), повідомлення не буде опубліковане, а ви більше не зможете мати доступ до публікації.";

            await client.EditMessageTextAsync(
                context.UserId,
                context.MessageId,
                message,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard.Build()
            );
        }
    }
}