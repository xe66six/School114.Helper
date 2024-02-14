using System.Threading.Tasks;
using SchoolHelper.Bot.Structs;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace SchoolHelper.Bot.RequestHandlers.Messages.DefaultHandlers
{
    public interface IDefaultHandler
    {
        string GetKey();
        Task ProcessRequest(ITelegramBotClient client, HandlerContext context, Query query);
    }
}