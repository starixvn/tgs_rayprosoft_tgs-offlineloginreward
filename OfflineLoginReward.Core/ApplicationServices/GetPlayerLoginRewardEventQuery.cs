namespace Tgs.OfflineLoginReward.ApplicationServices;

public record GetPlayerLoginRewardEventQuery(
	string PlayerCode,
	Guid PromotionId);
