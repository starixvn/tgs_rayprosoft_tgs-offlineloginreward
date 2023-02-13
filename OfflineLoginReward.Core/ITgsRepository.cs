namespace Tgs.OfflineLoginReward;

public interface ITgsRepository
{
	ValueTask<Guid> CreateOfflineLoginRewardRecordAsync(
		Guid promotionId, 
		string playerCode, 
		decimal bonusAmount, 
		decimal buckAmount, 
		DateTime loggedOn,
		CancellationToken cancellation = default);
}
