namespace Tgs.OfflineLoginReward;

public record OfflineLoginRewardEventInfo(
	Guid Id,
	string PlayerCode,
	Guid PromotionId,
	Guid BrandId,
	decimal BonusAmount,
	decimal BucketAmount,
	DateTime LoggedOn,
	DateTime CreatedOn);
