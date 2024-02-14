using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class BanHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/ban");

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var database = Database.Instance();
            
            var args = Helpers.ParseArguments(context.Content);
            var targetId = Convert.ToInt64(args["userid"]);
            database.BanUser(targetId);

            var closeKb = new InlineButtonBuilder()
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Закрити", "/delete")
                );
            
            await client.SendTextMessageAsync(
                targetId,
                "<b>😢 Ви були заблоковані.</b> Доступ до публікацій обмежено.",
                replyMarkup: closeKb.Build()
            );
            database.RemoveQueue(targetId);

            await client.EditMessageTextAsync(
                context.UserId,
                context.MessageId,
                $"Користувач {targetId} успішно заблокований",
                parseMode: ParseMode.Html
            );
        }
    }
}