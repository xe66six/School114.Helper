using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SchoolHelper.Bot.RequestHandlers.Messages.DefaultHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SchoolHelper.Bot.RequestHandlers.Messages
{
    public class DefaultHandler : IHandler
    {
        private static List<IDefaultHandler> _handlers;
        public static List<IDefaultHandler> Handlers() => _handlers ??= HandlerBroker.ListHandlers<IDefaultHandler>();
        
        public bool IsDefault() => true;
        
        public bool CanHandle(HandlerContext context) => context.Content.Length > 0;

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var db = Database.Instance();
            var query = db.FindQuery(context.UserId);

            if (string.IsNullOrEmpty(query.Key))
            {
                await client.SendTextMessageAsync(
                    context.UserId,
                    "Невідома команда"
                );
                return;
            }

            var handler = Handlers().FirstOrDefault(h => h.GetKey().Equals(query.Key));
            if (handler == null)
            {
                await client.SendTextMessageAsync(
                    context.UserId,
                    "Невідома команда"
                );
                return;
            }

            await handler.ProcessRequest(client, context, query);
        }
    }
}