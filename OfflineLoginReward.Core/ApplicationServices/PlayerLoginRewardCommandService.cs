using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginRewardCommandService : ICommandService<PlayerLoginRewardCommand>
{
	private readonly IPlayerServiceClient _playerServiceClient;
	private readonly IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> _promotionListQueryService;
	private readonly ICommandService<DistributeBonusCommand> _distributeService;
	private readonly IOfflineLoginRewardEventService _eventService;

	public PlayerLoginRewardCommandService(
		IPlayerServiceClient playerServiceClient,
		IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> promotionListQueryService,
		ICommandService<DistributeBonusCommand> distributeService,
		IOfflineLoginRewardEventService eventService)
	{
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
		_promotionListQueryService = promotionListQueryService ?? throw new ArgumentNullException(nameof(promotionListQueryService));
		_distributeService = distributeService ?? throw new ArgumentNullException(nameof(distributeService));
		_eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
	}

	public async ValueTask ExecuteAsync(
		PlayerLoginRewardCommand command, 
		CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		var promotions = await _promotionListQueryService
			.QueryAsync(new(), cancellationToken)
			.Where(p => p.EventDuring.StartTime <= now 
			            && p.EventDuring.EndTime > now 
			            && p.Settings.PlayerCodes.Contains(command.PlayerCode))
			.ToArrayAsync(cancellationToken)
			
			.ConfigureAwait(false);

		if (promotions.Length == 0)
			return;
		
		foreach (var promotion in promotions)
		{
			var rewardInfo = await _eventService
				.GetOfflineLoginRewardEventAsync(
					command.PlayerCode, 
					promotion.PromotionId, 
					cancellationToken)
				.ConfigureAwait(false);
			
			if (rewardInfo is not null)
				continue;

			var playerStatus = await _playerServiceClient
				.GetPlayerStatusAsync(command.PlayerCode, cancellationToken)
				.ConfigureAwait(false);

			if (playerStatus is not PlayerStatus.Enabled)
				continue;

			var isCreateEventProcessing = await _eventService
				.IsCreateOfflineLoginRewardEventProcessingAsync(
					command.PlayerCode, 
					promotion.PromotionId,
					cancellationToken)
				.ConfigureAwait(false);

			if (isCreateEventProcessing)
				continue;

			var bonusAmount = promotion.Settings.BonusAmount;
			var bucketAmount = bonusAmount * promotion.Settings.TurnoverMultiple;

			var rewardId = await _eventService
				.CreateOfflineLoginRewardEventAsync(
					new CreateOfflineLoginRewardEventInfo(
						command.PlayerCode,
						promotion.PromotionId,
						promotion.BrandId,
						bonusAmount,
						bucketAmount,
						command.LoggedInOn),
					cancellationToken)
				.ConfigureAwait(false);

			await _distributeService
				.ExecuteAsync(
					new DistributeBonusCommand(
						promotion.PromotionId,
						command.PlayerCode,
						bonusAmount,
						bucketAmount,
						rewardId),
					cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
