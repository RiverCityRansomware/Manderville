using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Manderville.Modules;

namespace Manderville {
    /// <summary>
    /// Detect whether a message is a command, then execute it.
    /// </summary>
    public class CommandHandler {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _provider;

        public async Task Install(IServiceProvider provider) {
            // Create a new instance of the commandservice.
            _commands = new CommandService();
            // Load all modules from the assembly
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

            _provider = provider;
            _client = provider.GetService<DiscordSocketClient>();

            // Register the MessageReceived event to handle commands
            _client.MessageReceived += HandleCommand;
        }

        private async Task HandleCommand(SocketMessage s) {
            // Check if the received message is from a user
            var msg = s as SocketUserMessage;
            if (msg == null) return;

            // Check if the message has either a string or mention prefix.
            int argPos = 0;

            //var settings = new GuildSettings();

            var context = new SocketCommandContext(_client, msg);
            //var name = context.Guild.Name;
            //Console.WriteLine($"name: {name}");
            if (!(msg.HasStringPrefix(Common.Configuration.Load().Prefix, ref argPos) || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            
            var result = await _commands.ExecuteAsync(context, argPos, _provider);
            Console.WriteLine($"{context.Guild.Name}: {msg}");
            // If execution failed, reply with the error message.
            if (!result.IsSuccess) {
                await context.Channel.SendMessageAsync("Thats an invalid Command, try `!m help`");
                Console.Out.WriteLine("Error: " + result.Error.ToString());
                Console.Out.WriteLine("Error Reason: " + result.ErrorReason.ToString());
                Console.Out.WriteLine("Success: " + result.IsSuccess);
            }

        }

    }
}
