using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class OfflineLoginRewardPromotionListQueryService 
	: IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>
{
	private readonly IPromotionServiceClient _promotionServiceClient;

	public OfflineLoginRewardPromotionListQueryService(
		IPromotionServiceClient promotionServiceClient)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
	}

	public IAsyncEnumerable<OfflineLoginRewardPromotion> QueryAsync(
		OfflineLoginRewardPromotionListQuery query,
		CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		return _promotionServiceClient
			.ListPromotionsByPromotionTypeAsync(
				"OfflineLoginReward",
				cancellationToken)
			.Where(info => info.EventDuring.From.AddHours(-1D) <= now
						   && info.EventDuring.To.AddHours(1D) > now)
			.Select(info => _promotionServiceClient
				.GetPromotionRuleAsync<PromotionRule>(info.Id, cancellationToken)
				.AsTask()
				.ToAsyncEnumerable()
				.Where(rule => rule is not null)
				.Select(rule => new OfflineLoginRewardPromotion(
					info.Id,
					info.BrandId,
					new EventDuring(info.EventDuring.From, info.EventDuring.To),
					rule!.CampaignDetail)))
			.Merge();
	}

	internal record PromotionRule(OfflineLoginRewardSettings CampaignDetail);
}
