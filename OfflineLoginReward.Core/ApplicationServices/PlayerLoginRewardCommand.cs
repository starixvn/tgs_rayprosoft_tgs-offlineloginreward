namespace Tgs.OfflineLoginReward.ApplicationServices;

public record PlayerLoginRewardCommand(
	string PlayerCode,
	Guid BrandId,
	DateTime LoggedInOn);
