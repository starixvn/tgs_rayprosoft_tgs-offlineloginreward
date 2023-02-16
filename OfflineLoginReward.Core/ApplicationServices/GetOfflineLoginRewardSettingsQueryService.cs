using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class GetOfflineLoginRewardSettingsQueryService 
	: IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?>
{
	private readonly IPromotionServiceClient _promotionServiceClient;

	public GetOfflineLoginRewardSettingsQueryService(
		IPromotionServiceClient promotionServiceClient)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
	}

	public async ValueTask<OfflineLoginRewardSettings?> GetAsync(
		GetOfflineLoginRewardSettingsQuery query,
		CancellationToken cancellationToken = default)
	{
		var rule = await _promotionServiceClient
			.GetPromotionRuleAsync<PromotionRule>(query.PromotionId, cancellationToken);

		return rule?.CampaignDetail;
	}

	internal class PromotionRule
	{
		public OfflineLoginRewardSettings CampaignDetail { get; init; } = default!;
	}
}
