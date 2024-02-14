using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SchoolHelper.Bot.Structs;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class ListPostsHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/list");

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var database = Database.Instance();
            var args = Helpers.ParseArguments(context.Content);

            var count = database.CountPosts();
            if (count == 0)
            {
                var kb = new InlineButtonBuilder()
                    .AddRow(
                        InlineKeyboardButton.WithCallbackData("Повернутися на головну", "/start")
                    );
                
                await client.EditMessageTextAsync(
                    context.UserId,
                    context.MessageId,
                    "Наразі ідей немає",
                    parseMode: ParseMode.Html,
                    replyMarkup: kb.Build()
                );
                return;
            }
            
            var post = database.GetLatestPost();
            if (args.TryGetValue("id", out var idStr))
            {
                var postId = Convert.ToInt32(idStr);
                post = database.FetchPost(postId);
            }
            
            if (args.TryGetValue("action", out var action))
            {
                var likes = database.GetLikes(post.Id);
                var dislikes = database.GetDislikes(post.Id);
                
                switch (action)
                {
                    case "like":
                    {
                        database.AddLike(post.Id, context.UserId);
                        var updatedLikes = database.GetLikes(post.Id);
                        if (likes == updatedLikes)
                        {
                            await client.AnswerCallbackQueryAsync(
                                context.CallbackQueryId,
                                "Ви вже залишили лайк для даної ідеї"
                            );
                            return;
                        }
                        break;
                    }

                    case "dislike":
                    {
                        database.AddDislike(post.Id, context.UserId);
                        var updatedDislikes = database.GetDislikes(post.Id);
                        if (dislikes == updatedDislikes)
                        {
                            await client.AnswerCallbackQueryAsync(
                                context.CallbackQueryId,
                                "Ви вже залишили дизлайк для даної ідеї"
                            );
                            return;
                        }
                        break;
                    }
                }
            }

            var prev = database.PreviousPost(post.Id);
            var next = database.NextPost(post.Id);

            var navigation = new List<InlineKeyboardButton>();
            if (prev.OwnerId > 0) navigation.Add(
                InlineKeyboardButton.WithCallbackData("⬅️", "/list?id=" + prev.Id)
                );
            if (next.OwnerId > 0) navigation.Add(
                InlineKeyboardButton.WithCallbackData("➡️️", "/list?id=" + next.Id)
            );
            
            var keyboard = new InlineButtonBuilder()
                .AddRow(
                    navigation.ToArray()
                )
                .AddRow(
                    InlineKeyboardButton.WithCallbackData($"{database.GetLikes(post.Id)} 👍", "/list?id=" + post.Id + "&action=like"),
                    InlineKeyboardButton.WithCallbackData($"{database.GetDislikes(post.Id)} 👎", "/list?id=" + post.Id + "&action=dislike")
                )
                .AddRow(
                    InlineKeyboardButton.WithCallbackData("Повернутися на головну", "/start")
                );

            var user = database.GetUser(post.OwnerId);
            
            await client.EditMessageTextAsync(
                context.UserId,
                context.MessageId,
                $"<b>Автор ідеї:</b> {user.Surname} {user.Name} ({user.Form}-{user.FormLetter})\r\n\r\n{post.Content}",
                parseMode: ParseMode.Html,
                replyMarkup: keyboard.Build()
            );

        }
    }
}