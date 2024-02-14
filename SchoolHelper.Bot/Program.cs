using System;
using Microsoft.Extensions.Logging;
using NLog;

namespace SchoolHelper.Bot
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var tg = new TelegramBotWrapper(Config.BotToken);
            Console.ReadKey();
        }
    }
}