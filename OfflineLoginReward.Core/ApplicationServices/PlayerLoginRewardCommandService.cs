using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginRewardCommandService : ICommandService<PlayerLoginRewardCommand>
{
	private readonly PromotionListDataStore _promotionListDataStore;
	private readonly IPlayerLoginRewardHandler _playerLoginRewardHandler;

	public PlayerLoginRewardCommandService(
		PromotionListDataStore promotionListDataStore,
		IPlayerLoginRewardHandler playerLoginRewardHandler)
	{
		_promotionListDataStore = promotionListDataStore ?? throw new ArgumentNullException(nameof(promotionListDataStore));
		_playerLoginRewardHandler = playerLoginRewardHandler ?? throw new ArgumentNullException(nameof(playerLoginRewardHandler));
	}

	public async ValueTask ExecuteAsync(
		PlayerLoginRewardCommand command, 
		CancellationToken cancellationToken = default)
	{
		var now = DateTime.UtcNow;

		var promotions = await _promotionListDataStore
			.GetPromotionListAsync(cancellationToken)
			.Where(p => p.EventDuring.StartTime <= now 
				&& p.EventDuring.EndTime > now)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);

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
