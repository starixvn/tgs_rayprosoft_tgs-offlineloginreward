namespace Tgs.OfflineLoginReward;

internal class OfflineLoginRewardEventService : IOfflineLoginRewardEventService
{
	private readonly ITgsRepository _tgsRepository;

	public OfflineLoginRewardEventService(
		ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default)
		=> _tgsRepository.GetOfflineLoginRewardEventAsync(playerCode, promotionId, cancellationToken);

	public ValueTask<Guid> CreateOfflineLoginRewardEventAsync(CreateOfflineLoginRewardEventInfo createInfo,
		CancellationToken cancellationToken = default) 
		=> _tgsRepository.CreateOfflineLoginRewardEventAsync(createInfo, cancellationToken);
}
