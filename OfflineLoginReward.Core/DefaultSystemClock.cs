namespace Tgs.OfflineLoginReward;

internal class DefaultSystemClock : ISystemClock
{
	public DateTime UtcNow => DateTime.UtcNow;
}
