using System;

namespace SchoolHelper.Bot
{
    public class Config
    {
        public static string BotToken = Environment.GetEnvironmentVariable("token");
        public static long OwnerId = 883910722;
    }
}