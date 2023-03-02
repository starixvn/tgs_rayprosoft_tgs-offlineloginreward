namespace Tgs.OfflineLoginReward;

internal interface IOfflineLoginRewardPromotionListStore
{
	IAsyncEnumerable<OfflineLoginRewardPromotion> QueryOfflineLoginRewardPromotionListAsync(
		CancellationToken cancellationToken = default);
}
