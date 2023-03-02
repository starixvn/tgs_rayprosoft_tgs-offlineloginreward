namespace Tgs.OfflineLoginReward;

internal class OfflineLoginRewardEventService : IOfflineLoginRewardEventService
{
	private readonly ITgsRepository _tgsRepository;
	private readonly IOfflineLoginRewardEventRepository _eventRepository;

	public OfflineLoginRewardEventService(
		ITgsRepository tgsRepository,
		IOfflineLoginRewardEventRepository eventRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
		_eventRepository = eventRepository;
	}

	public async ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default)
	{
		var info = await _eventRepository.GetOfflineLoginRewardEventAsync(
				playerCode, promotionId, cancellationToken)
			.ConfigureAwait(false);

		if (info is not null)
			return info; 

		info = await _tgsRepository
			.GetOfflineLoginRewardEventAsync(playerCode, promotionId, cancellationToken)
			.ConfigureAwait(false);

		if (info is not null)
			await _eventRepository.SetOfflineLoginRewardEventAsync(playerCode, promotionId, info!, cancellationToken)
			.ConfigureAwait(false);

		return info;
	}

	public async ValueTask<Guid> CreateOfflineLoginRewardEventAsync(
		
		CreateOfflineLoginRewardEventInfo createInfo,
		CancellationToken cancellationToken = default)
	{
		await _eventRepository
			.BeginCreateOfflineLoginRewardEventAsync(createInfo.PlayerCode, createInfo.PromotionId, cancellationToken)
			.ConfigureAwait(false);

		var eventId = await _tgsRepository
			.CreateOfflineLoginRewardEventAsync(createInfo, cancellationToken)
			.ConfigureAwait(false);

		var info = await _tgsRepository
			.GetOfflineLoginRewardEventAsync(eventId, cancellationToken)
			.ConfigureAwait(false);

		await _eventRepository
			.SetOfflineLoginRewardEventAsync(createInfo.PlayerCode, createInfo.PromotionId, info!, cancellationToken)
			.ConfigureAwait(false);

		await _eventRepository
			.CompleteCreateOfflineLoginRewardEventAsync(createInfo.PlayerCode, createInfo.PromotionId, cancellationToken)
			.ConfigureAwait(false);

		return eventId;
	}

	public ValueTask<bool> IsCreateOfflineLoginRewardEventProcessingAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default)
		=> _eventRepository
			.IsProcessingCreateOfflineLoginRewardEventAsync(playerCode, promotionId, cancellationToken);
}
