namespace Tgs.OfflineLoginReward;

public interface ITgsRepository
{
	ValueTask<Guid> CreateOfflineLoginRewardEventAsync(
		CreateOfflineLoginRewardEventInfo createEventInfo,
		CancellationToken cancellationToken = default);
}
