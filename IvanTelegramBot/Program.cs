using IvanTelegramBot;
using Telegram.Bot;

var botClient = new TelegramBotClient(AccessTokens.Telegram);

var me = await botClient.GetMeAsync();
Console.WriteLine($"Hello, World! I am bot {me.Id} and my name is {me.FirstName}.");

var bot = new BotEngine(botClient);
await bot.ListenForMessagesAsync();

//await bot.SendMessageAsync();