using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SchoolHelper.Bot.RequestHandlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SchoolHelper.Bot
{
    public class TelegramBotWrapper
    {
        public TelegramBotWrapper(string token)
        {
            var bot = new TelegramBotClient(token);
            var logger = LogManager.GetCurrentClassLogger();
            
            var getMeTask = bot.GetMeAsync();
            getMeTask.Wait();
            var botCredentials = getMeTask.Result;
            
            logger.Info("Authorized as @{0}", botCredentials.Username);
            
            bot.StartReceiving(
                HandleUpdateAsync,
                HandlePollingErrorsAsync
                );
            
            logger.Info("Started polling");
        }
        
        private async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            LogUserRequest(update);
            await HandlerBroker.ForwardUpdate(client, update);
        }
        
        private Task HandlePollingErrorsAsync(ITelegramBotClient client, Exception exception, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        private void LogUserRequest(Update update)
        {
            User u;
            string content;
            
            switch (update.Type)
            {
                case UpdateType.Message:
                    u = update.Message!.From;
                    content = update.Message!.Text;
                    break;
                
                case UpdateType.CallbackQuery:
                    u = update.CallbackQuery!.From;
                    content = update.CallbackQuery!.Data;
                    break;
                
                default:
                    u = default;
                    content = string.Empty;
                    break;
            }

            if (u == default) return;
            
            Database.Instance().AddUserIfNotExists(u!.Id);
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug("{0} >> {1} | {2}", u!.Id, content, update.Type.ToString());
        }
    }
}