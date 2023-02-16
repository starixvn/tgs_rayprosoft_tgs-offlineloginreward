using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginCommandService : ICommandService<PlayerLoginCommand>
{
	private readonly ISystemClock _systemClock;
	private readonly IPlayerService _playerService;
	private readonly IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo> _promotionQueryService;
	private readonly IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?> _promotionSettingsQueryService;

	private readonly ICommandService<DistributeBonusCommand> _distributeService;
	private readonly ITgsRepository _tgsRepository;

	public PlayerLoginCommandService(
		ISystemClock systemClock,
		IPlayerService playerService,
		IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo> promotionListQueryService,
		IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?> promotionSettingsQueryService,
		ICommandService<DistributeBonusCommand> distributeService,
		ITgsRepository tgsRepository)
	{
		_systemClock = systemClock ?? throw new ArgumentNullException(nameof(systemClock));
		_playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
		_promotionQueryService = promotionListQueryService ?? throw new ArgumentNullException(nameof(promotionListQueryService));
		_promotionSettingsQueryService = promotionSettingsQueryService ?? throw new ArgumentNullException(nameof(promotionSettingsQueryService));
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

		var promotions = await _promotionQueryService
			.QueryAsync(new(), cancellationToken)
			.Where(p => p.BrandId == playerInfo.BrandId 
			            && p.EventDuring.From <= utcNow 
			            && p.EventDuring.To > utcNow)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		if (promotions.Length == 0)
			return;
		
		foreach (var promotion in promotions)
		{
			var settings = await _promotionSettingsQueryService.GetAsync(
					new GetOfflineLoginRewardSettingsQuery(promotion.Id),
					cancellationToken)
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

			await _distributeService
				.ExecuteAsync(
					new DistributeBonusCommand(
						promotion.Id,
						command.PlayerCode,
						bonusAmount,
						bucketAmount,
						rewardId),
					cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
