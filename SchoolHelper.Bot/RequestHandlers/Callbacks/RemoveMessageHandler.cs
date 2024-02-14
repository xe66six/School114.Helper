using System.Threading.Tasks;
using Telegram.Bot;

namespace SchoolHelper.Bot.RequestHandlers.Callbacks
{
    public class RemoveMessageHandler : IHandler
    {
        public bool IsDefault() => false;
        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/delete");

        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            await client.DeleteMessageAsync(
                context.UserId,
                context.MessageId
            );
        }
    }
}