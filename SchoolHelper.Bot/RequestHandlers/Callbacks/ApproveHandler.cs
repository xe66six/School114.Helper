using System;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class ApproveHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/approve");

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var database = Database.Instance();
            var args = Helpers.ParseArguments(context.Content);
            var postId = Convert.ToInt32(args["id"]);
            var post = database.FetchPost(postId);

            await client.DeleteMessageAsync( context.UserId, context.MessageId);

            var closeKb = new InlineButtonBuilder()
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Закрити", "/delete")
                );
            
            switch (Convert.ToBoolean(args["value"]))
            {
                case true:
                {
                    database.ApprovePost(postId);

                    await client.SendTextMessageAsync(
                        post.OwnerId,
                        "✅ Ваш пост був <b>одобрений!</b>",
                        parseMode: ParseMode.Html,
                        replyMarkup: closeKb.Build()
                    );
                    
                    break;
                }

                case false:
                {
                    await client.SendTextMessageAsync(
                        post.OwnerId,
                        "❌ Ваш пост був <b>відхилений!</b>",
                        parseMode: ParseMode.Html,
                        replyMarkup: closeKb.Build()
                    );
                    break;
                }
            }
        }
    }
}