using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class OfflineLoginRewardPromotionListQueryService : IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>
{
	private readonly IPromotionServiceClient _promotionServiceClient;

	public OfflineLoginRewardPromotionListQueryService(
		IPromotionServiceClient promotionServiceClient)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
	}

	public IAsyncEnumerable<PromotionMainInfo> QueryAsync(
		OfflineLoginRewardPromotionListQuery query,
		CancellationToken cancellationToken = default) 
		=> _promotionServiceClient.ListPromotionsByPromotionTypeAsync(
			OfflineLoginRewardUtility.PromotionType, 
			cancellationToken);
}
