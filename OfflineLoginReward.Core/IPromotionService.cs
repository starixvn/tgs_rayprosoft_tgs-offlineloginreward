using Tgs.Promotion.Clients;

namespace Tgs.OfflineLoginReward;

public interface IPromotionService
{
	IAsyncEnumerable<PromotionMainInfo> GetOfflineLoginRewardPromotionsAsync(
		Guid brandId, 
		CancellationToken cancellationToken = default);

	ValueTask<OfflineLoginRewardSettings?> GetOfflineLoginRewardSettingsAsync(
		Guid promotionId, 
		CancellationToken cancellationToken = default);
}
