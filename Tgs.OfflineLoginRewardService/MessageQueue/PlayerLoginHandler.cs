using Tgs.MessageQueue;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Operator.Client.Protos;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.MessageQueue;

public class PlayerLoginHandler : IMessageHandler
{
	private readonly IPlayerServiceClient _playerServiceClient;
	private readonly IGetService<GetPlayerInfoQuery, PlayerInfo?> _playerQueryService;
	private readonly ICommandService<PlayerLoginRewardCommand> _commandService;

	public PlayerLoginHandler(
		IPlayerServiceClient playerServiceClient,
		IGetService<GetPlayerInfoQuery, PlayerInfo?> playerQueryService,
		ICommandService<PlayerLoginRewardCommand> commandService)
	{
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
		_playerQueryService = playerQueryService ?? throw new ArgumentNullException(nameof(playerQueryService));
		_commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
	}

	public async ValueTask HandleAsync(
		ReadOnlyMemory<byte> data, 
		CancellationToken cancellationToken = default)
	{
		var loginEvent = LoggedInEvent.Parser.ParseFrom(data.Span);

		if (loginEvent is null)
			return;

		var playerInfo = await _playerQueryService
			.GetAsync(
				new GetPlayerInfoQuery(loginEvent.PlayerCode),
				cancellationToken)
			.ConfigureAwait(false);

		if (playerInfo is null)
			return;

		var playerStatus = await _playerServiceClient
			.GetPlayerStatusAsync(playerInfo.PlayerCode, cancellationToken)
			.ConfigureAwait(false);

		if (playerStatus is not PlayerStatus.Enabled)
			return;

		var command = new PlayerLoginRewardCommand(
			playerInfo.PlayerCode,
			playerInfo.BrandId,
			loginEvent.LoggedInTime.ToDateTime());

		await _commandService
			.ExecuteAsync(command, cancellationToken)
			.ConfigureAwait(false);
	}
}
