using System.Threading.Tasks;
using SchoolHelper.Bot.RequestHandlers.Messages;
using Telegram.Bot;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class StartCallbackHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/start");

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var database = Database.Instance();
            database.RemoveQueue(context.UserId);
            
            var handler = new StartHandler();
            await handler.ProcessRequest(client, context.RequireEdit());
        }
    }
}