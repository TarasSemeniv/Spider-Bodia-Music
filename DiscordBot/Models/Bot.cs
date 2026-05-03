using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Models;

public class Bot : IBot
{
    private ServiceProvider? _serviceProvider;
    
    private readonly ILogger<Bot> _logger;
    private readonly IConfiguration _config;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;

    public Bot(
        ILogger<Bot> logger,
        IConfiguration config, 
        DiscordSocketClient client,
        InteractionService interactionService)
    {
        _logger = logger;
        _config = config;
        _client = client;
        _interactionService = interactionService;
    }

    public async Task StartAsync(ServiceProvider services)
    {
        _serviceProvider = services;
        string discordToken = _config["Discord:Token"]
                              ?? throw new NullReferenceException("Missing discord token");
        
        var interactionService = _serviceProvider.GetRequiredService<InteractionService>();
        
        await interactionService.AddModulesAsync(
            Assembly.GetExecutingAssembly(),
            _serviceProvider);
        
        _client.InteractionCreated += async interaction =>
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
        };
        
        _client.Log += msg =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };
        
        _client.Ready += async () =>
        {
            try 
            {
                // Реєструємо команди для конкретного сервера (Guild)
                ulong guildId = 895643733771550740; // Замініть на ID вашого сервера
                await interactionService.RegisterCommandsToGuildAsync(guildId);
            
                // Альтернатива: Реєстрація команд глобально (для всіх серверів).
                // Увага: глобальна реєстрація може зайняти до години часу на стороні Discord!
                // await interactionService.RegisterCommandsGloballyAsync();
            
                _logger.LogInformation("Commands registered successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to register commands: {ex.Message}");
            }
        };

        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();
        
        
        _logger.LogInformation("Bot started");
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Shutting down");

        await _client.LogoutAsync();
        await _client.StopAsync();
    }
}