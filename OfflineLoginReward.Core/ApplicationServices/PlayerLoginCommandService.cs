using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginCommandService : ICommandService<PlayerLoginCommand>
{
	private readonly ISystemClock _systemClock;
	private readonly IPromotionService _promotionService;
	private readonly IDistributeService _distributeService;
	private readonly ITgsRepository _tgsRepository;

	public PlayerLoginCommandService(
		ISystemClock systemClock,
		IPromotionService promotionService,
		IDistributeService distributeService,
		ITgsRepository tgsRepository)
	{
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));
		_distributeService = distributeService ?? throw new ArgumentNullException(nameof(distributeService));
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(
		PlayerLoginCommand command, 
		CancellationToken cancellationToken = default)
	{
		var utcNow = _systemClock.UtcNow;

		var promotions = await _promotionService
			.GetOfflineLoginRewardPromotionsAsync(cancellationToken)
			.Where(p => p.BrandId == command.BrandId 
			            && p.EventDuring.From <= utcNow 
			            && p.EventDuring.To > utcNow)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		if (promotions.Length == 0)
			return;

		foreach (var promotion in promotions)
		{
			var settings = await _promotionService
				.GetOfflineLoginRewardSettingsAsync(promotion.Id, cancellationToken)
				.ConfigureAwait(false);

			if (settings is null || !settings.PlayerCodes.Contains(command.PlayerCode))
				continue;

			var bonusAmount = settings.BonusAmount;
			var bucketAmount = bonusAmount * settings.TurnoverMultiple;

			var rewardId = await _tgsRepository
				.CreateOfflineLoginRewardEventAsync(
					new CreateOfflineLoginRewardEventInfo(
						command.PlayerCode,
						promotion.Id,
						command.BrandId,
						bonusAmount,
						bucketAmount,
						utcNow),
					cancellationToken)
				.ConfigureAwait(false);


			var plan = await _distributeService
				.GetDistributePlanInfoAsync(promotion.Id, cancellationToken)
				.ConfigureAwait(false);
			
			_ = await _distributeService
				.DistributeBonusAsync(
					plan.Id, 
					command.PlayerCode,
					bonusAmount,
					bucketAmount,
					rewardId,
					cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
