using Discord.Interactions;

namespace DiscordBot.Commands.Echo;

public class EchoCommand : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("echo", "Echoes a message.")]
    public async Task ExecuteAsync(
        [Summary(description: "A phrase")] string phrase)
    {
        if (string.IsNullOrEmpty(phrase))
        {
            await RespondAsync("Please enter a phrase", ephemeral: true);
            return;
        }
        
        await RespondAsync(phrase);
    }
}