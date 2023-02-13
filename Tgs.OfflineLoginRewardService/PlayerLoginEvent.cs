namespace Tgs.OfflineLoginReward;

public record PlayerLoginEvent(
	Guid BrandId, 
	string PlayerCode);
