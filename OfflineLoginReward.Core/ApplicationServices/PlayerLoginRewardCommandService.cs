using Microsoft.Extensions.Logging;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class PlayerLoginRewardCommandService : ICommandService<PlayerLoginRewardCommand>
{
	private readonly IPromotionListDataStore _promotionListDataStore;
	private readonly IPlayerLoginRewardHandler _playerLoginRewardHandler;
	private readonly ILogger<PlayerLoginRewardCommandService> _logger;

	public PlayerLoginRewardCommandService(
		IPromotionListDataStore promotionListDataStore,
		IPlayerLoginRewardHandler playerLoginRewardHandler,
		ILogger<PlayerLoginRewardCommandService> logger)
	{
		_promotionListDataStore = promotionListDataStore ?? throw new ArgumentNullException(nameof(promotionListDataStore));
		_playerLoginRewardHandler = playerLoginRewardHandler ?? throw new ArgumentNullException(nameof(playerLoginRewardHandler));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	public async ValueTask ExecuteAsync(
		PlayerLoginRewardCommand command, 
		CancellationToken cancellationToken = default)
	{
		_logger.LogInformation("OnEvent");

		var now = DateTime.UtcNow;

		var promotions = await _promotionListDataStore
			.GetPromotionListAsync(cancellationToken)
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
