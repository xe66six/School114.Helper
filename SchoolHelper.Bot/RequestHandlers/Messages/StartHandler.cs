using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot.RequestHandlers.Messages
{
    public class StartHandler : IHandler
    {
        public bool IsDefault() => false;

        public bool CanHandle(HandlerContext context) => context.Content.StartsWith("/start");

        private Dictionary<bool, InlineButtonBuilder> _keyboardMarkups = new Dictionary<bool, InlineButtonBuilder>
        {
            {
                true, // user: signed up
                new InlineButtonBuilder()
                    .AddRow(
                        InlineKeyboardButton.WithCallbackData("Переглянути ідеї", "/list"),
                        InlineKeyboardButton.WithCallbackData("Мої дані", "/profile")
                    )
                    .AddRow(
                        InlineKeyboardButton.WithCallbackData("Створити ідею", "/create")
                    )
            },
            
            {
                false, // user: new
                new InlineButtonBuilder()
                    .AddRow(
                        InlineKeyboardButton.WithCallbackData("Заповнити дані", "/signup")
                    )
            }
        };
        
        public async Task ProcessRequest(ITelegramBotClient client, HandlerContext context)
        {
            var database = Database.Instance();
            var user = database.GetUser(context.UserId);
            var signedUp = user.CompletelyFilled();
            
            database.RemoveQueue(user.UserId);

            var keyboard = _keyboardMarkups[signedUp];

            var message =
                "<b>Привіт, друже! 👋</b> Даний Telegram-бот допоможе запропонувати твою ідею щодо покращення школи, або підтримати вже існуючу ідею.\r\n\r\n" +
                "Кожен має змогу створити ідею, або прокоментувати вже існуючу. Скористайтеся кнопками нижче для початку роботи з ботом.\r\n\r\n" +
                "Якщо контент має образливий, негативний, безглуздий, або дискримінаціний характер, публікація буде відхилена, а користувач буде позбавлений права створювати нові ідеї.\r\n\r\n" +
                "<code>Бот розроблений учнем 11-Б класу Машкіном Романом</code>";
            
            if (context.CanEditMessage)
            {
                await client.EditMessageTextAsync(
                    context.UserId,
                    context.MessageId,
                    message,
                    parseMode: ParseMode.Html,
                    replyMarkup: keyboard.Build()
                );
                return;
            }
            
            await client.SendTextMessageAsync(
                context.UserId,
                message,
                parseMode: ParseMode.Html,
                replyMarkup: keyboard.Build()
            );
        }
    }
}