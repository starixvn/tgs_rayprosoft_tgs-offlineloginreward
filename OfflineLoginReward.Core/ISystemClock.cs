namespace Tgs.OfflineLoginReward;

internal interface ISystemClock
{
	DateTime UtcNow { get; }
}
