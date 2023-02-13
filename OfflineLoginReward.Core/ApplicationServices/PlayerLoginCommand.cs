namespace Tgs.OfflineLoginReward.ApplicationServices;

public record PlayerLoginCommand(
	Guid BrandId,
	string PlayerCode);
