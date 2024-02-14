using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using SchoolHelper.Bot.RequestHandlers.Messages;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class SignUpHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/signup");

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var db = Database.Instance();

            var args = Helpers.ParseArguments(context.Content);
            if (args.Count == 0)
            {
                db.AddToQueue(context.UserId, "name", context.MessageId);
            
                await client.EditMessageTextAsync(
                    context.UserId,
                    context.MessageId,
                    "<b>Давайте знайомитися!</b> Введіть ваше ім'я",
                    parseMode: ParseMode.Html
                );
            }
            else
            {
                var newValue = args.ElementAt(0);
                switch (newValue.Key)
                {
                    case "form":
                    {
                        db.UpdateUserCredentials(context.UserId, "form", newValue.Value);
                        db.RemoveQueue(context.UserId);

                        var keyboard = new InlineButtonBuilder()
                            .AddRow(
                                "АБВГ".ToCharArray()
                                    .Select(ch => InlineKeyboardButton.WithCallbackData(ch.ToString(), "/signup?form_letter="+ch))
                                    .ToArray()
                            );
                        
                        await client.EditMessageTextAsync(
                            context.UserId,
                            context.MessageId,
                            "<b>Зрозуміло</b> Оберіть літеру вашого классу",
                            parseMode: ParseMode.Html,
                            replyMarkup: keyboard.Build()
                        );
                        break;
                    }
                    case "form_letter":
                    {
                        db.UpdateUserCredentials(context.UserId, "form_letter", newValue.Value);
                        
                        await client.EditMessageTextAsync(
                            context.UserId,
                            context.MessageId,
                            "<b>✅ Ви успішно зареєструвались</b>",
                            parseMode: ParseMode.Html
                        );

                        var startHandler = new StartCallbackHandler();
                        context.CanEditMessage = false;
                        await startHandler.ProcessRequest(client, context);
                        
                        break;
                    }
                }
            }
        }
    }
}