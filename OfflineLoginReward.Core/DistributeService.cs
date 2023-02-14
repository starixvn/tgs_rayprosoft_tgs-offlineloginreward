using Tgs.Promotion.Clients;

namespace Tgs.OfflineLoginReward;

internal class DistributeService : IDistributeService
{
	private readonly IPromotionServiceClient _promotionServiceClient;
	
	public DistributeService(
		IPromotionServiceClient promotionServiceClient)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
	}

	public ValueTask<PromotionDistributePlanInfo> GetDistributePlanInfoAsync(
		Guid promotionId, 
		CancellationToken cancellationToken = default)
	{
		var conditionHash = 1000L;

		return _promotionServiceClient
			.GetPromotionDistributePlanAsync(
				promotionId,
				conditionHash, 
				cancellationToken) ;
	}

	public ValueTask<Guid> DistributeBonusAsync(
		Guid planId,
		string playerCode,
		decimal bonusAmount,
		decimal bucketAmount,
		Guid rewardId,
		CancellationToken cancellationToken = default)
		=> _promotionServiceClient.DistributePromotionBonusAsync(
			planId,
			playerCode,
			bonusAmount,
			bucketAmount,
			null,
			rewardId,
			cancellationToken);
}
