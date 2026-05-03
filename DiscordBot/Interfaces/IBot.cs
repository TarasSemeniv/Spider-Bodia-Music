using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Interfaces;

public interface IBot
{
    Task StartAsync(ServiceProvider services);
    Task StopAsync();
}