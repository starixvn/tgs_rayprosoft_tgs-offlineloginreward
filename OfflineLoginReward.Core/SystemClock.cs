namespace Tgs.OfflineLoginReward;

internal class SystemClock : ISystemClock
{
	public DateTime UtcNow => DateTime.UtcNow;
}
