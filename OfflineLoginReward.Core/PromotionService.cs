using Tgs.Promotion.Clients;

namespace Tgs.OfflineLoginReward;

internal class PromotionService : IPromotionService
{
	private readonly IPromotionServiceClient _promotionServiceClient;

	public PromotionService(
		IPromotionServiceClient promotionServiceClient)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
	}

	public IAsyncEnumerable<PromotionMainInfo> GetOfflineLoginRewardPromotionsAsync(
		CancellationToken cancellationToken = default)
		=> _promotionServiceClient
			.ListPromotionsByPromotionTypeAsync(OfflineLoginRewardUtility.PromotionType, cancellationToken);

	public async ValueTask<OfflineLoginRewardSettings?> GetOfflineLoginRewardSettingsAsync(
		Guid promotionId, 
		CancellationToken cancellationToken = default)
	{
		var rule = await _promotionServiceClient.GetPromotionRuleAsync<PromotionRule>(promotionId, cancellationToken);

		return rule?.CampaignDetail;
	}
	
	internal class PromotionRule
	{
		public OfflineLoginRewardSettings CampaignDetail { get; init; } = default!;
	}
}
