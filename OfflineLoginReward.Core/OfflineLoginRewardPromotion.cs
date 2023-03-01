namespace Tgs.OfflineLoginReward;

public record OfflineLoginRewardPromotion(
	Guid PromotionId,
	Guid BrandId,
	EventDuring EventDuring,
	OfflineLoginRewardSettings Settings);
