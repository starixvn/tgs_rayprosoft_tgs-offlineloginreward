namespace Tgs.OfflineLoginReward;

internal interface IOfflineLoginRewardEventService
{
	ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default);
	
	ValueTask<Guid> CreateOfflineLoginRewardEventAsync(
		CreateOfflineLoginRewardEventInfo createInfo, 
		CancellationToken cancellationToken = default);
}
