using Tgs.OfflineLoginReward.Repositories.Entities;
using Tgs.SerialNumberGenerator;

namespace Tgs.OfflineLoginReward.Repositories;

internal class TgsRepository : ITgsRepository
{
	private readonly ISerialNumberGenerator _serialNumberGenerator;
	private readonly TgsContext _tgsContext;

	public TgsRepository(
		ISerialNumberGenerator serialNumberGenerator,
		TgsContext tgsContext)
	{
		_serialNumberGenerator = serialNumberGenerator ?? throw new ArgumentNullException(nameof(serialNumberGenerator));
		_tgsContext = tgsContext ?? throw new ArgumentNullException(nameof(tgsContext));
	}

	public async ValueTask<Guid> CreateOfflineLoginRewardEventAsync(
		CreateOfflineLoginRewardEventInfo createEventInfo,
		CancellationToken cancellationToken = default)
	{
		var eventId = await _serialNumberGenerator.GenerateAsync(cancellationToken).ConfigureAwait(false);

		var rewardEvent = new OfflineLoginRewardEvent
		{
			Id = eventId,
			PromotionId = createEventInfo.PromotionId,
			PlayerCode = createEventInfo.PlayerCode,
			BrandId = createEventInfo.BrandId, 
			Bonus = createEventInfo.BonusAmount,
			Bucket = createEventInfo.BucketAmount,
			LoggedOn = createEventInfo.LoggedOn
		};

		_ = await _tgsContext.OfflineLoginRewardEvents
			.AddAsync(rewardEvent, cancellationToken)
			.ConfigureAwait(false);

		_ = await _tgsContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

		return eventId;
	}
}
