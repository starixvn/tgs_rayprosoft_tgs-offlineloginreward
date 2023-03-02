using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward;

internal class PlayerLoginRewardHandler : IPlayerLoginRewardHandler
{
	private readonly IPlayerServiceClient _playerServiceClient;
	private readonly ICommandService<DistributeBonusCommand> _distributeService;
	private readonly IOfflineLoginRewardEventService _eventService;

	public PlayerLoginRewardHandler(
		IPlayerServiceClient playerServiceClient,
		ICommandService<DistributeBonusCommand> distributeService,
		IOfflineLoginRewardEventService eventService)
	{
		_playerServiceClient = playerServiceClient ?? throw new ArgumentNullException(nameof(playerServiceClient));
		_distributeService = distributeService ?? throw new ArgumentNullException(nameof(distributeService));
		_eventService = eventService ?? throw new ArgumentNullException(nameof(eventService));
	}

	public async ValueTask OnDistributeRewardAsync(
		string playerCode,
		OfflineLoginRewardPromotion promotion,
		DateTime loggedOn,
		CancellationToken cancellationToken = default)
	{
		if (!promotion.Settings.PlayerCodes.Contains(playerCode))
			return;

		var isArchived = await _eventService
			.IsEventArchivedAsync(
				playerCode, 
				promotion.PromotionId,
				promotion.EventDuring.EndTime,
				cancellationToken)
			.ConfigureAwait(false);

		if (isArchived)
			return;

		var playerStatus = await _playerServiceClient
			.GetPlayerStatusAsync(playerCode, cancellationToken)
			.ConfigureAwait(false);

		if (playerStatus is not PlayerStatus.Enabled)
			return;
		
		var bonusAmount = promotion.Settings.BonusAmount;
		var bucketAmount = bonusAmount * promotion.Settings.TurnoverMultiple;

		var rewardId = await _eventService
			.CreateOfflineLoginRewardEventAsync(
				new CreateOfflineLoginRewardEventInfo(
					playerCode,
					promotion.PromotionId,
					promotion.BrandId,
					bonusAmount,
					bucketAmount,
					loggedOn),
				cancellationToken)
			.ConfigureAwait(false);

		await _distributeService
			.ExecuteAsync(
				new DistributeBonusCommand(
					promotion.PromotionId,
					playerCode,
					bonusAmount,
					bucketAmount,
					rewardId),
				cancellationToken)
			.ConfigureAwait(false);
	}
}
