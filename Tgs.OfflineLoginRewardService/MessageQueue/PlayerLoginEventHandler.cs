using Tgs.MessageQueue;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Operator.Client.Protos;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.MessageQueue;

public class PlayerLoginEventHandler : IMessageHandler
{
	private readonly ICommandService<PlayerLoginRewardCommand> _commandService;

	public PlayerLoginEventHandler(
		ICommandService<PlayerLoginRewardCommand> commandService)
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

		var command = new PlayerLoginRewardCommand(
			loginEvent.PlayerCode,
			loginEvent.LoggedInTime.ToDateTime());

		await _commandService
			.ExecuteAsync(command, cancellationToken)
			.ConfigureAwait(false);
	}
}
