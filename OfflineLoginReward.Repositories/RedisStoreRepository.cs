using StackExchange.Redis;

namespace Tgs.OfflineLoginReward.Repositories;

internal class RedisStoreRepository : IStoreRepository
{
	private readonly IDatabase _redisDatabase;

	public RedisStoreRepository(
		IDatabase redisDatabase)
	{
		_redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
	}
	
	public async ValueTask<bool> SetEventArchivedAsync(
		string playerCode, 
		Guid promotionId, 
		DateTime promotionEndTime,
		CancellationToken cancellationToken = default)
	{
		var key = $"OfflineLoginRewardEvent:{promotionId}:{playerCode}";
		var expiry = promotionEndTime.ToUniversalTime().AddDays(1) - DateTime.UtcNow;

		return await _redisDatabase.StringSetAsync(
			key,
			true,
			expiry,
			when: When.NotExists).ConfigureAwait(false);
	}
}
