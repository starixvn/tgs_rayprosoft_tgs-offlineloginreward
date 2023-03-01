namespace Tgs.OfflineLoginReward;

public record OfflineLoginRewardSettings(
	decimal BonusAmount,
	decimal TurnoverMultiple,
	HashSet<string> PlayerCodes);

