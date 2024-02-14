using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SchoolHelper.Bot.RequestHandlers
{
    public interface IHandler
    {
        bool IsDefault();
        bool CanHandle(HandlerContext context);
        Task ProcessRequest(ITelegramBotClient client, HandlerContext context);
    }
}