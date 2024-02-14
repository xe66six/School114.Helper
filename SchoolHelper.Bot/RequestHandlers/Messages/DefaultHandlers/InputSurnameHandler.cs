using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolHelper.Bot.Structs;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Messages.DefaultHandlers
{
    public class InputSurnameHandler : IDefaultHandler
    {
        public string GetKey() => "surname";

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context, Query query)
        {
            await client.DeleteMessageAsync(context.UserId, context.MessageId);
            
            // TODO: check if surname is suitable
            
            var db = Database.Instance();
            db.UpdateUserCredentials(context.UserId, "surname", context.Content);
            
            db.RemoveQueue(context.UserId);
            db.AddToQueue(context.UserId, "form", query.MessageId);

            var variants = Enumerable.Range(1, 12).ToList();

            var keyboard = new InlineButtonBuilder();
            for (var i = 0; i < variants.Count; i += 6)
            {
                keyboard.AddRow(
                    variants
                        .Skip(i)
                        .Take(6)
                        .Select(x => InlineKeyboardButton.WithCallbackData(x.ToString(), "/signup?form=" + x))
                        .ToArray()
                );
            }
            
            await client.EditMessageTextAsync(
                context.UserId,
                query.MessageId,
                "<b>Гаразд!</b> В якому класі ти навчаєшся?",
                parseMode: ParseMode.Html,
                replyMarkup: keyboard.Build()
            );
        }
    }
}