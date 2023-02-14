using Tgs.Promotion.Clients;

namespace Tgs.OfflineLoginReward;

internal interface IDistributeService
{
	ValueTask<PromotionDistributePlanInfo> GetDistributePlanInfoAsync(
		Guid promotionId, 
		CancellationToken cancellationToken = default);

	ValueTask<Guid> DistributeBonusAsync(
		Guid planId, 
		string playerCode, 
		decimal bonusAmount, 
		decimal bucketAmount, 
		Guid rewardId, 
		CancellationToken cancellationToken = default);
}
