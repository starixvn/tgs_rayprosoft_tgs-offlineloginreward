using Tgs.PlayerService.Client;

namespace Tgs.OfflineLoginReward;

public interface IPlayerService
{
	ValueTask<PlayerInfo?> GetPlayerInfoAsync(string playerCode, CancellationToken cancellationToken = default);

	ValueTask<PlayerStatus> GetPlayerStatusAsync(string playerCode, CancellationToken cancellationToken = default);
}
