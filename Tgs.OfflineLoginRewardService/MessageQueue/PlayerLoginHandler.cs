using System.Text.Json;
using Tgs.MessageQueue;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.MessageQueue;

public class PlayerLoginHandler : IMessageHandler
{
	private readonly ICommandService<PlayerLoginCommand> _commandService;

	public PlayerLoginHandler(
		ICommandService<PlayerLoginCommand> commandService)
	{
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public async ValueTask HandleAsync(
		ReadOnlyMemory<byte> data, 
		CancellationToken cancellationToken = default)
	{
		var loginEvent = JsonSerializer.Deserialize<PlayerLoginEvent>(data.Span);

		if (loginEvent is null)
			return;
		
		var command = new PlayerLoginCommand(
			loginEvent.BrandId, 
			loginEvent.PlayerCode);

		await _commandService
			.ExecuteAsync(command, cancellationToken)
			.ConfigureAwait(false);
	}
}
