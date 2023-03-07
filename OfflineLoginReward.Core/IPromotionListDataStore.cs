namespace Tgs.OfflineLoginReward;

internal interface IPromotionListDataStore
{
	IAsyncEnumerable<OfflineLoginRewardPromotion> GetPromotionListAsync(
		CancellationToken cancellationToken = default);
}
