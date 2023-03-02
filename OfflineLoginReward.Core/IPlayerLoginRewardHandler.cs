namespace Tgs.OfflineLoginReward;

internal interface IPlayerLoginRewardHandler
{
	ValueTask OnDistributeRewardAsync(
		string playerCode,
		OfflineLoginRewardPromotion promotion,
		DateTime loggedOn,
		CancellationToken cancellationToken = default);
}
