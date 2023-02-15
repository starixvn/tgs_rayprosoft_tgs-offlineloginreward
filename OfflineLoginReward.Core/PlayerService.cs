using Tgs.PlayerService.Client;

namespace Tgs.OfflineLoginReward;

internal class PlayerService : IPlayerService
{
	private readonly IPlayerServiceClient _playerServiceClient;

	public PlayerService(
		IPlayerServiceClient playerServiceClient)
	{
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
	}

	public ValueTask<PlayerInfo?> GetPlayerInfoAsync(
		string playerCode,
		CancellationToken cancellationToken = default) 
		=> _playerServiceClient.GetPlayerInfoAsync(playerCode, cancellationToken);

	public ValueTask<PlayerStatus> GetPlayerStatusAsync(
		string playerCode,
		CancellationToken cancellationToken = default)
		=> _playerServiceClient.GetPlayerStatusAsync(playerCode, cancellationToken);
}
