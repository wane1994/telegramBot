using Microsoft.Graph.Beta.Drives.Item.Items.Item.Workbook.Worksheets.Item.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace IvanTelegramBot
{
    public class BotEngine
    {
        private readonly TelegramBotClient _botClient;
        private const long AdminChatId = 248254479;

        public BotEngine(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task ListenForMessagesAsync()
        {
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
            };
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await _botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case UpdateType.Message:
                        await HandleMessageAsync(update.Message);
                        break;
                    case UpdateType.CallbackQuery:
                        await HandleCallbackQueryAsync(update.CallbackQuery);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        private async Task HandleMessageAsync(Message message)
        {
            var user = message.From;
            var chat = message.Chat;

            LogMessage(user, message);

            switch (message.Type)
            {
                case MessageType.Text:
                    await HandleTextMessageAsync(message, user, chat);
                    break;
                default:
                    await _botClient.SendTextMessageAsync(chat.Id, "Use only text!");
                    break;
            }
        }

        private async Task HandleTextMessageAsync(Message message, User user, Chat chat)
        {
            switch (message.Text)
            {
                case "/start":
                    await _botClient.SendTextMessageAsync(chat.Id, "Choose a keyboard:\n/inline\n/reply");
                    break;
                case "/inline":
                    await SendInlineKeyboardAsync(chat.Id);
                    break;
                case "/reply":
                    await SendReplyKeyboardAsync(chat.Id);
                    break;
                case "Call me!":
                    await _botClient.SendTextMessageAsync(chat.Id, "Alright, send me your number!", replyToMessageId: message.MessageId);
                    break;
                case "Write to my neighbor!":
                    await _botClient.SendTextMessageAsync(chat.Id, "Can't you do it yourself?", replyToMessageId: message.MessageId);
                    break;
            }
        }

        private async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            var user = callbackQuery.From;
            var chat = callbackQuery.Message.Chat;

            LogCallbackQuery(user, callbackQuery);

            switch (callbackQuery.Data)
            {
                case "button1":
                    await HandleButton1Async(callbackQuery, chat);
                    break;
                case "button2":
                    await HandleButton2Async(callbackQuery, chat);
                    break;
                case "button3":
                    await HandleButton3Async(callbackQuery, chat);
                    break;
            }
        }

        private async Task HandleButton1Async(CallbackQuery callbackQuery, Chat chat)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
            await _botClient.SendTextMessageAsync(chat.Id, $"You pressed {callbackQuery.Data}");
        }

        private async Task HandleButton2Async(CallbackQuery callbackQuery, Chat chat)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Here could be your text!");
            await _botClient.SendTextMessageAsync(chat.Id, $"You pressed {callbackQuery.Data}");
        }

        private async Task HandleButton3Async(CallbackQuery callbackQuery, Chat chat)
        {
            await _botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "This is a full-screen text!", showAlert: true);
            await _botClient.SendTextMessageAsync(chat.Id, $"You pressed {callbackQuery.Data}");
        }

        private async Task SendInlineKeyboardAsync(long chatId)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                new List<InlineKeyboardButton[]>
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithUrl("This is a button with a website", "https://habr.com/"),
                        InlineKeyboardButton.WithCallbackData("And this is just a button", "button1"),
                    },
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Another one here", "button2"),
                        InlineKeyboardButton.WithCallbackData("And here", "button3"),
                    },
                });

            await _botClient.SendTextMessageAsync(chatId, "This is an inline keyboard!", replyMarkup: inlineKeyboard);
        }

        private async Task SendReplyKeyboardAsync(long chatId)
        {
            var replyKeyboard = new ReplyKeyboardMarkup(
                new List<KeyboardButton[]>
                {
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Hello!"),
                        new KeyboardButton("Goodbye!"),
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Call me!")
                    },
                    new KeyboardButton[]
                    {
                        new KeyboardButton("Write to my neighbor!")
                    }
                })
            {
                ResizeKeyboard = true,
            };

            await _botClient.SendTextMessageAsync(chatId, "This is a reply keyboard!", replyMarkup: replyKeyboard);
        }

        private void LogMessage(User user, Message message)
        {
            Console.WriteLine($"{user.FirstName} ({user.Id}) sent a message: {message.Text}");
        }

        private void LogCallbackQuery(User user, CallbackQuery callbackQuery)
        {
            Console.WriteLine($"{user.FirstName} ({user.Id}) pressed the button: {callbackQuery.Data}");
        }

        private void LogException(Exception exception)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            LogException(exception);
            return Task.CompletedTask;
        }
    }
}
