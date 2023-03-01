namespace Tgs.OfflineLoginReward.ApplicationServices;

public record PlayerLoginRewardCommand(
	string PlayerCode,
	DateTime LoggedInOn);
