namespace Tgs.OfflineLoginReward.ApplicationServices;

public record DistributeBonusCommand(
	Guid PromotionId,
	string PlayerCode,
	decimal BonusAmount,
	decimal BucketAmount,
	Guid RewardRecordId);
