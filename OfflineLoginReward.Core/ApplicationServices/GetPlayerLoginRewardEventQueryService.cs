using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class GetPlayerLoginRewardEventQueryService : IGetService<GetPlayerLoginRewardEventQuery, OfflineLoginRewardEventInfo?>
{
	private readonly ITgsRepository _tgsRepository;

	public GetPlayerLoginRewardEventQueryService(
		ITgsRepository tgsRepository)
	{
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public ValueTask<OfflineLoginRewardEventInfo?> GetAsync(
		GetPlayerLoginRewardEventQuery query,
		CancellationToken cancellationToken = default)
		=> _tgsRepository.GetOfflineLoginRewardEventAsync(query.PlayerCode, query.PromotionId, cancellationToken);
}
