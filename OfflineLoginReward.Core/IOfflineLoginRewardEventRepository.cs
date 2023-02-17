namespace Tgs.OfflineLoginReward;

public interface IOfflineLoginRewardEventRepository
{
	ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default);

	ValueTask SetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		OfflineLoginRewardEventInfo eventInfo,
		CancellationToken cancellationToken = default);

	ValueTask BeginCreateOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default);

	ValueTask CompleteCreateOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default);

	ValueTask<bool> IsProcessingCreateOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default);
}
