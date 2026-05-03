using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordBot.Interfaces;
using DiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DiscordBot;

public class Program
{
    public static void Main(string[] args)
    {
        MainAsync(args).GetAwaiter().GetResult();
    }

    private static async Task MainAsync(string[] args)
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets(Assembly.GetExecutingAssembly())
            .Build();

        var services = new ServiceCollection()
            .AddLogging(opt =>
            {
                opt.ClearProviders();
                opt.AddConsole();
            })
            .AddSingleton<IConfiguration>(config)
            .AddSingleton<DiscordSocketClient>(x =>
                new DiscordSocketClient(new DiscordSocketConfig
                {
                    GatewayIntents =
                        GatewayIntents.Guilds |
                        GatewayIntents.GuildMessages
                }))
            .AddSingleton(x =>
                {
                    var client = x.GetRequiredService<DiscordSocketClient>();
                    return new InteractionService(client);
                })
            .AddSingleton<IBot, Bot>()
            .BuildServiceProvider();
        
        try
        {
            var bot = services.GetRequiredService<IBot>();

            await bot.StartAsync(services);
            
            do
            {
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Q)
                {
                    await bot.StopAsync();
                    return;
                }
            } while (true);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            Environment.Exit(-1);
        }
    }
}