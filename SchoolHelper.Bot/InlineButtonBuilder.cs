using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;

namespace SchoolHelper.Bot
{
    public class InlineButtonBuilder
    {
        private List<List<InlineKeyboardButton>> _buttons = new List<List<InlineKeyboardButton>>();
        
        public InlineButtonBuilder AddRow(params InlineKeyboardButton[] buttons)
        {
            _buttons.Add(buttons.ToList());
            return this;
        }

        public InlineKeyboardMarkup Build() => new InlineKeyboardMarkup(_buttons);
    }
}