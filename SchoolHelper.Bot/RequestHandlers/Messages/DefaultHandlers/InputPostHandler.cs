using System.Threading.Tasks;
using SchoolHelper.Bot.Structs;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Messages.DefaultHandlers
{
    public class InputPostHandler : IDefaultHandler
    {
        public string GetKey() => "new_post";

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context, Query query)
        {
            await client.DeleteMessageAsync(context.UserId, context.MessageId);
            
            var database = Database.Instance();
            database.RemoveQueue(context.UserId);

            var postid = database.CreatePost(context.UserId, context.Content);
            var message = "Пост відправлено на перевірку. Очікуйте підтвердження.";

            var keyboard = new InlineButtonBuilder()
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Повернутися на головну", "/start")
                );
            
            await client.EditMessageTextAsync(
                context.UserId,
                query.MessageId,
                message,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard.Build()
            );

            message = $"Новий пост на перевірку\r\nАвтор: {context.UserId}\r\n\r\n{context.Content}";
            var kb = new InlineButtonBuilder()
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Відхилити", "/approve?value=false&id=" + postid),
                    InlineKeyboardButton.WithCallbackData("Одобрити", "/approve?value=true&id=" + postid)
                )
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Заблокувати користувача", "/ban?userid=" + context.UserId)
                );

            await client.SendTextMessageAsync(
                Config.OwnerId,
                message,
                parseMode: ParseMode.Html,
                replyMarkup: kb.Build()
            );
        }
    }
}