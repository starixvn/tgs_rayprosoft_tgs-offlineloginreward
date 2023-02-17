namespace Tgs.OfflineLoginReward;

public interface ITgsRepository
{
	ValueTask<Guid> CreateOfflineLoginRewardEventAsync(
		CreateOfflineLoginRewardEventInfo createEventInfo,
		CancellationToken cancellationToken = default);

	ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default);

	ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		Guid eventId,
		CancellationToken cancellationToken = default);
}
