using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginRewardCommandService : ICommandService<PlayerLoginRewardCommand>
{
	private readonly IQueryService<QualifiedOfflineLoginRewardPromotionListQuery, PromotionMainInfo> _promotionQueryService;
	private readonly IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?> _promotionSettingsQueryService;
	private readonly ICommandService<DistributeBonusCommand> _distributeService;
	private readonly IOfflineLoginRewardEventService _eventService;

	public PlayerLoginRewardCommandService(
		IQueryService<QualifiedOfflineLoginRewardPromotionListQuery, PromotionMainInfo> promotionListQueryService,
		IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?> promotionSettingsQueryService,
		ICommandService<DistributeBonusCommand> distributeService,
		IOfflineLoginRewardEventService eventService)
	{
		_promotionQueryService = promotionListQueryService ?? throw new ArgumentNullException(nameof(promotionListQueryService));
		_promotionSettingsQueryService = promotionSettingsQueryService ?? throw new ArgumentNullException(nameof(promotionSettingsQueryService));
		_distributeService = distributeService ?? throw new ArgumentNullException(nameof(distributeService));
		_eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
	}

	public async ValueTask ExecuteAsync(
		PlayerLoginRewardCommand command, 
		CancellationToken cancellationToken = default)
	{
		var promotions = await _promotionQueryService
			.QueryAsync(new(command.BrandId), cancellationToken)
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

			var rewardInfo = await _eventService
				.GetOfflineLoginRewardEventAsync(
					command.PlayerCode, 
					promotion.Id, 
					cancellationToken)
				.ConfigureAwait(false);
			
			if (rewardInfo is not null)
				continue;
			
			var bonusAmount = settings.BonusAmount;
			var bucketAmount = bonusAmount * settings.TurnoverMultiple;

			var rewardId = await _eventService
				.CreateOfflineLoginRewardEventAsync(
					new CreateOfflineLoginRewardEventInfo(
						command.PlayerCode,
						promotion.Id,
						command.BrandId,
						bonusAmount,
						bucketAmount,
						command.LoggedInOn),
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
