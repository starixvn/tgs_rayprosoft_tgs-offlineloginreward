namespace Tgs.OfflineLoginReward;

public interface IStoreRepository
{
	ValueTask<bool> SetEventArchivedAsync(
		string playerCode,
		Guid promotionId,
		DateTime promotionEndTime,
		CancellationToken cancellationToken = default);
}
