using System.Threading.Tasks;
using SchoolHelper.Bot.Structs;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SchoolHelper.Bot.RequestHandlers.Messages.DefaultHandlers
{
    public class InputNameHandler : IDefaultHandler
    {
        public string GetKey() => "name";

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context, Query query)
        {
            await client.DeleteMessageAsync(context.UserId, context.MessageId);
            
            // TODO: check if name is suitable
            
            var db = Database.Instance();
            db.UpdateUserCredentials(context.UserId, "name", context.Content);
            
            db.RemoveQueue(context.UserId);
            db.AddToQueue(context.UserId, "surname", query.MessageId);

            await client.EditMessageTextAsync(
                context.UserId,
                query.MessageId,
                "<b>Чудово!</b> Тепер введіть своє прізвище",
                parseMode: ParseMode.Html
            );
        }
    }
}