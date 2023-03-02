using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginRewardCommandService : ICommandService<PlayerLoginRewardCommand>
{
	private readonly IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> _promotionListQueryService;
	private readonly IPlayerLoginRewardHandler _playerLoginRewardHandler;

	public PlayerLoginRewardCommandService(
		IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> promotionListQueryService,
		IPlayerLoginRewardHandler playerLoginRewardHandler)
	{
		_promotionListQueryService = promotionListQueryService ?? throw new ArgumentNullException(nameof(promotionListQueryService));
		_playerLoginRewardHandler = playerLoginRewardHandler ?? throw new ArgumentNullException(nameof(playerLoginRewardHandler));
	}

	public async ValueTask ExecuteAsync(
		PlayerLoginRewardCommand command, 
		CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		var promotions = await _promotionListQueryService
			.QueryAsync(new(), cancellationToken)
			.Where(p => p.EventDuring.StartTime <= now 
			            && p.EventDuring.EndTime > now)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

		if (promotions.Length == 0)
			return;
		
		foreach (var promotion in promotions)
		{
			await _playerLoginRewardHandler
				.OnDistributeRewardAsync(
					command.PlayerCode,
					promotion,
					command.LoggedInOn,
					cancellationToken)
				.ConfigureAwait(false);
		}
	}
}
