using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class QualifiedOfflineLoginRewardPromotionListQueryService :
	IQueryService<QualifiedOfflineLoginRewardPromotionListQuery, PromotionMainInfo>
{
	private readonly ISystemClock _systemClock;
	private readonly IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo> _queryService;

	public QualifiedOfflineLoginRewardPromotionListQueryService(
		ISystemClock systemClock,
		IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo> queryService)
	{
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_queryService = queryService ?? throw new ArgumentNullException(nameof(queryService));
	}

	public IAsyncEnumerable<PromotionMainInfo> QueryAsync(
		QualifiedOfflineLoginRewardPromotionListQuery query,
		CancellationToken cancellationToken = default)
	{
		var utcNow = _systemClock.UtcNow;

		return _queryService
			.QueryAsync(new OfflineLoginRewardPromotionListQuery(), cancellationToken)
			.Where(p => p.EventDuring.From <= utcNow 
			            && p.EventDuring.To > utcNow);
	}
}
