using System.Reflection;
using System.Text.Json;
using StackExchange.Redis;

namespace Tgs.OfflineLoginReward.Repositories;

internal class RedisRepository : IOfflineLoginRewardEventRepository
{
	private static readonly JsonSerializerOptions _SerializerOptions = new()
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private readonly IDatabase _redisDatabase;

	public RedisRepository(
		IDatabase redisDatabase)
	{
		_redisDatabase = redisDatabase ?? throw new ArgumentNullException(nameof(redisDatabase));
	}

	public async ValueTask<OfflineLoginRewardEventInfo?> GetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default)
	{
		var key = $"OfflineLoginRewardEvent:{playerCode}:{promotionId}";

		var value = await _redisDatabase.StringGetAsync(key).ConfigureAwait(false);

		return value.HasValue
			? JsonSerializer.Deserialize<OfflineLoginRewardEventInfo>(value!, _SerializerOptions)
			: null;
	}

	public async ValueTask SetOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		OfflineLoginRewardEventInfo eventInfo,
		CancellationToken cancellationToken = default)
	{
		var key = $"OfflineLoginRewardEvent:{playerCode}:{promotionId}";

		_ = await _redisDatabase.StringSetAsync(
			key,
			JsonSerializer.Serialize(eventInfo, _SerializerOptions),
			TimeSpan.FromDays(1D));
	}

	public async ValueTask BeginCreateOfflineLoginRewardEventAsync(
		string playerCode,
		Guid promotionId,
		CancellationToken cancellationToken = default)
	{
		var key = $"OfflineLoginRewardEvent:CreateLock:{playerCode}:{promotionId}";

		_ = await _redisDatabase.StringSetAsync(
			key,
			"true",
			when: When.NotExists).ConfigureAwait(false);
	}

	public async ValueTask CompleteCreateOfflineLoginRewardEventAsync(
		string playerCode, 
		Guid promotionId,
		CancellationToken cancellationToken = default)
	{
		var key = $"OfflineLoginRewardEvent:CreateLock:{playerCode}:{promotionId}";

		_ = await _redisDatabase.KeyDeleteAsync(key).ConfigureAwait(false);
	}

	public async ValueTask<bool> IsProcessingCreateOfflineLoginRewardEventAsync(string playerCode, Guid promotionId,
		CancellationToken cancellationToken = default)
	{
		var key = $"OfflineLoginRewardEvent:CreateLock:{playerCode}:{promotionId}";

		return await _redisDatabase.KeyExistsAsync(key).ConfigureAwait(false);
	}
}
