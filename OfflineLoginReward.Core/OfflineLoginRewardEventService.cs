namespace Tgs.OfflineLoginReward;

internal class OfflineLoginRewardEventService : IOfflineLoginRewardEventService
{
	private readonly ITgsRepository _tgsRepository;
	private readonly IStoreRepository _storeRepository;

	public OfflineLoginRewardEventService(
		ITgsRepository tgsRepository,
		IStoreRepository storeRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
	}

	public ValueTask<Guid> CreateOfflineLoginRewardEventAsync(
		CreateOfflineLoginRewardEventInfo createInfo,
		CancellationToken cancellationToken = default)
		=> _tgsRepository
			.CreateOfflineLoginRewardEventAsync(createInfo, cancellationToken);

	public async ValueTask<bool> IsEventArchivedAsync(
		string playerCode, 
		Guid promotionId, 
		DateTime promotionEndTime,
		CancellationToken cancellationToken = default)
	{
		var isSet = await _storeRepository
			.SetEventArchivedAsync(playerCode, promotionId, promotionEndTime, cancellationToken)
			.ConfigureAwait(false);

		if (!isSet)
			return true;

		var rewardEvent = await _tgsRepository
			.GetOfflineLoginRewardEventAsync(playerCode, promotionId, cancellationToken)
			.ConfigureAwait(false);

		if (rewardEvent is null) 
			return false;

		_ = await _storeRepository
			.SetEventArchivedAsync(playerCode, promotionId, promotionEndTime, cancellationToken)
			.ConfigureAwait(false);

		return true;

	}
}
