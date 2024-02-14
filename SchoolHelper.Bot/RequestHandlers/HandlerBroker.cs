using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SchoolHelper.Bot.RequestHandlers
{
    public class HandlerBroker
    {
        private static List<IHandler> _handlers;
        public static List<IHandler> Handlers() => _handlers ??= HandlerBroker.ListHandlers<IHandler>();
        public static List<T> ListHandlers<T>()
        {
            var assembly = Assembly.GetExecutingAssembly();

            var ret = 
                from t in assembly.GetTypes()
                where typeof(T).IsAssignableFrom(t) && !t.IsInterface
                select (T)Activator.CreateInstance(t);

            return ret.ToList();
        }

        public static async Task ForwardUpdate(ITelegramBotClient client, Update update)
        {
            var context = HandlerContext.Parse(update);
            var handlers = HandlerBroker.Handlers().Where(handler => handler.CanHandle(context)).ToList();
            
            if (handlers == null)
                throw new Exception("Couldn't find suitable handler for Telegram request");
            
            var handler = handlers.First();
            if (handlers.Count > 1)
                handler = handlers.First(x => !x.IsDefault());

            try
            {
                await handler.ProcessRequest(client, context);
            }
            catch (Exception ex)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Error(ex);
            }
        }
    }
}