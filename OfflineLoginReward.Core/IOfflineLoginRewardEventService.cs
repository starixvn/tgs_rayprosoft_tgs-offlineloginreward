namespace Tgs.OfflineLoginReward;

internal interface IOfflineLoginRewardEventService
{
	ValueTask<Guid> CreateOfflineLoginRewardEventAsync(
		CreateOfflineLoginRewardEventInfo createInfo,
		CancellationToken cancellationToken = default);

	ValueTask<bool> IsEventArchivedAsync(
		string playerCode,
		Guid promotionId,
		DateTime promotionEndTime,
		CancellationToken cancellationToken = default);
}
