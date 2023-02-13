namespace Tgs.OfflineLoginReward;

public record OfflineLoginRewardSettings(
	decimal BonusAmount,
	decimal TurnoverMultiple,
	IEnumerable<string> PlayerCodes);

