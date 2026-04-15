using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using CheckLiveBot.Services;

namespace CheckLiveBot
{
    internal class Program
    {
        private static ITelegramBotClient _botClient;
        private static CancellationTokenSource _cts;
        private static DatabaseService _databaseService;
        private static AuthorizationService _authService;
        private static MessageHandler _messageHandler;
        private static CallbackQueryHandler _callbackHandler;

        // ✅ GIỮ NGUYÊN TOKEN
        private static readonly string BotToken = "8644559117:AAHbLNGyxWL6_KY8kbRqi_CM5aSymC3kPRE";

        static async Task Main(string[] args)
        {
            try
            {
                _databaseService = new DatabaseService();
                await _databaseService.InitializeDatabaseAsync();

                _authService = new AuthorizationService(_databaseService);
                _messageHandler = new MessageHandler(_databaseService, _authService);
                _callbackHandler = new CallbackQueryHandler(_databaseService, _authService);

                _botClient = new TelegramBotClient(BotToken);
                _cts = new CancellationTokenSource();

                _botClient.StartReceiving(
                    HandleUpdateAsync,
                    HandlePollingErrorAsync,
                    new ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
                    _cts.Token
                );

                var me = await _botClient.GetMe();
                Console.WriteLine($"Bot @{me.Username} đã khởi động.");

                // 🔥 FIX QUAN TRỌNG: giữ app chạy
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                    await _messageHandler.ProcessAsync(bot, update.Message, ct);
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
                    await _callbackHandler.ProcessAsync(bot, update.CallbackQuery, ct);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling update: {ex.Message}");
            }
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
        {
            Console.WriteLine($"Polling error: {ex.Message}");
            return Task.CompletedTask;
        }
    }
}
