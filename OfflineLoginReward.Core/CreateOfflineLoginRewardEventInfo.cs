namespace Tgs.OfflineLoginReward;

public record CreateOfflineLoginRewardEventInfo(
	string PlayerCode,
	Guid PromotionId, 
	Guid BrandId,
	decimal BonusAmount,
	decimal BucketAmount,
	DateTime LoggedOn);
