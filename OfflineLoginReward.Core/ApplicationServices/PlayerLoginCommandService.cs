using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginCommandService : ICommandService<PlayerLoginCommand>
{
	private readonly ISystemClock _systemClock;
	private readonly IPromotionService _promotionService;
	private readonly IPlayerService _playerService;
	private readonly IDistributeService _distributeService;
	private readonly ITgsRepository _tgsRepository;

	public PlayerLoginCommandService(
		ISystemClock systemClock,
		IPromotionService promotionService,
		IPlayerService playerService,
		IDistributeService distributeService,
		ITgsRepository tgsRepository)
	{
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));
		_playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
		_distributeService = distributeService ?? throw new ArgumentNullException(nameof(distributeService));
		_tgsRepository = tgsRepository ?? throw new ArgumentNullException(nameof(tgsRepository));
	}

	public async ValueTask ExecuteAsync(
		PlayerLoginCommand command, 
		CancellationToken cancellationToken = default)
	{
		var playerInfo = await _playerService.GetPlayerInfoAsync(command.PlayerCode, cancellationToken)
			.ConfigureAwait(false);

		if (playerInfo is null)
			return;

		var playerStatus = await _playerService.GetPlayerStatusAsync(playerInfo.PlayerCode, cancellationToken)
			.ConfigureAwait(false);

		if (playerStatus is not PlayerStatus.Enabled)
			return;

		var utcNow = _systemClock.UtcNow;

		var promotions = await _promotionService
			.GetOfflineLoginRewardPromotionsAsync(cancellationToken)
			.Where(p => p.BrandId == playerInfo.BrandId 
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

			var rewardInfo = await _tgsRepository
				.GetOfflineLoginRewardEventAsync(
					command.PlayerCode, 
					promotion.Id, 
					cancellationToken)
				.ConfigureAwait(false);
			
			if (rewardInfo is not null)
				continue;
			
			var bonusAmount = settings.BonusAmount;
			var bucketAmount = bonusAmount * settings.TurnoverMultiple;

			var rewardId = await _tgsRepository
				.CreateOfflineLoginRewardEventAsync(
					new CreateOfflineLoginRewardEventInfo(
						command.PlayerCode,
						promotion.Id,
						playerInfo.BrandId,
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
