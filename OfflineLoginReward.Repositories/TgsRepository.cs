using Microsoft.EntityFrameworkCore;
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

	public async ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		string playerCode, 
		Guid promotionId,
		CancellationToken cancellationToken = default) 
		=> await _tgsContext.OfflineLoginRewardEvents
			.Where(x => 
				x.PlayerCode == playerCode && 
				x.PromotionId == promotionId)
			.Select(x => new OfflineLoginRewardEventInfo(
				x.Id,
				x.PlayerCode,
				x.PromotionId,
				x.BrandId,
				x.Bonus,
				x.Bucket,
				x.LoggedOn,
				x.CreatedOn))
			.FirstOrDefaultAsync(cancellationToken)
			.ConfigureAwait(false);
}
