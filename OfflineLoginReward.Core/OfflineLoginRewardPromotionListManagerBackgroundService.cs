using Microsoft.Extensions.Hosting;

namespace Tgs.OfflineLoginReward;

internal class OfflineLoginRewardPromotionListManagerBackgroundService : BackgroundService
{
	private readonly OfflineLoginRewardPromotionListManager _manager;

	public OfflineLoginRewardPromotionListManagerBackgroundService(
		OfflineLoginRewardPromotionListManager manager)
	{
		_manager = manager ?? throw new ArgumentNullException(nameof(manager));
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		stoppingToken.Register(_manager.OnStop);

		return Task.Run(async () =>
		{
			await _manager.InitAsync(stoppingToken).ConfigureAwait(false);
		}, stoppingToken);
	}
}
