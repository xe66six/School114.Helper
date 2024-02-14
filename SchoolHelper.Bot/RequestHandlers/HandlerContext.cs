using System;
using System.Data.SQLite;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SchoolHelper.Bot.RequestHandlers
{
    public class HandlerContext
    {
        public long UserId { get; set; }
        public string Content { get; set; }
        public bool CanEditMessage { get; set; }
        public int MessageId { get; set; }
        public bool EditRequired { get; set; }
        public string CallbackQueryId { get; set; }

        public static HandlerContext Parse(Update update)
        {
            return update.Type switch
            {
                UpdateType.Message => new HandlerContext
                {
                    UserId = update.Message!.From!.Id,
                    Content = update.Message!.Text,
                    CanEditMessage = false,
                    MessageId = update.Message!.MessageId,
                    CallbackQueryId = null
                },
                UpdateType.CallbackQuery => new HandlerContext
                {
                    UserId = update.CallbackQuery!.From!.Id,
                    Content = update.CallbackQuery!.Data,
                    CanEditMessage = true,
                    MessageId = update.CallbackQuery!.Message!.MessageId,
                    CallbackQueryId = update.CallbackQuery!.Id
                },
                _ => throw new NotImplementedException("Unimplemented update type")
            };
        }

        public HandlerContext RequireEdit()
        {
            this.EditRequired = true;
            return this;
        }
    }
}