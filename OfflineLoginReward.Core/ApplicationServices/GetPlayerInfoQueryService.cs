using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class GetPlayerInfoQueryService : IGetService<GetPlayerInfoQuery, PlayerInfo?>
{
	private readonly IPlayerServiceClient _playerServiceClient;

	public GetPlayerInfoQueryService(
		IPlayerServiceClient playerServiceClient)
	{
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
	}

	public ValueTask<PlayerInfo?> GetAsync(
		GetPlayerInfoQuery query,
		CancellationToken cancellationToken = default)
		=> _playerServiceClient.GetPlayerInfoAsync(query.PlayerCode, cancellationToken);
}
