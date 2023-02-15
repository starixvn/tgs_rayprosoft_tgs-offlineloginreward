using System.Text.Json;
using Tgs.MessageQueue;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Operator.Client.Protos;
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
		var loginEvent = LoggedInEvent.Parser.ParseFrom(data.Span);

		if (loginEvent is null)
			return;
		
		var command = new PlayerLoginCommand(
			loginEvent.PlayerCode,
			loginEvent.LoggedInTime.ToDateTime());

		await _commandService
			.ExecuteAsync(command, cancellationToken)
			.ConfigureAwait(false);
	}
}
