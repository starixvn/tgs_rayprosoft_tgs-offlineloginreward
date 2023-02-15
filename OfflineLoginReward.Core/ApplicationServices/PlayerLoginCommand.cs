namespace Tgs.OfflineLoginReward.ApplicationServices;

public record PlayerLoginCommand(
	string PlayerCode,
	DateTime LoggedInOn);
