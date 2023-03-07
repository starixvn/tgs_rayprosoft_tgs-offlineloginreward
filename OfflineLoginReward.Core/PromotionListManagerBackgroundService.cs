using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward;

internal class PromotionListManagerBackgroundService : BackgroundService
{
	private readonly IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> _queryService;
	private readonly PromotionListManager _manager;
	private readonly TimeSpan _promotionListRenewPeriod;

	public PromotionListManagerBackgroundService(
		IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> queryService,
		PromotionListManager manager,
		IOptions<PromotionListRenewOptions> options)
	{
		_queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
		_manager = manager ?? throw new ArgumentNullException(nameof(manager));

		if (options is null)
			throw new ArgumentNullException(nameof(options));

		_promotionListRenewPeriod = TimeSpan.FromSeconds(options.Value.PeriodBySeconds);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var timer = new PeriodicTimer(_promotionListRenewPeriod);

		_ =	stoppingToken.Register(timer.Dispose);

		do
		{
			var promotions = await _queryService
				.QueryAsync(new OfflineLoginRewardPromotionListQuery(), stoppingToken)
				.ToArrayAsync(stoppingToken)
				.ConfigureAwait(false);

			_manager.RenewPromotionList(promotions);
		} while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false)
				 && !stoppingToken.IsCancellationRequested);
	}
}
