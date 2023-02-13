namespace Tgs.OfflineLoginReward;

public record OfflineLoginRewardSettings(
	decimal BonusAmount,
	decimal TurnoverMultiple,
	string[] PlayerCodes);

