using Microsoft.Extensions.Caching.Memory;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.Cache;

internal class GetCachedPlayerInfoQueryService : IGetService<GetPlayerInfoQuery, PlayerInfo?>
{
	private readonly IGetService<GetPlayerInfoQuery, PlayerInfo?> _sourceService;
	private readonly IMemoryCache _memoryCache;

	public GetCachedPlayerInfoQueryService(
		IGetService<GetPlayerInfoQuery, PlayerInfo?> sourceService,
		IMemoryCache memoryCache)
	{
		_sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	public async ValueTask<PlayerInfo?> GetAsync(
		GetPlayerInfoQuery query, 
		CancellationToken cancellationToken = default)
	{
		var key = $"PlayerInfo:{query.PlayerCode}";

		return await _memoryCache.GetOrCreateAsync(key, async entry =>
		{
			entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);

			return await _sourceService.GetAsync(query, cancellationToken)
				.ConfigureAwait(false);
		});
	}
}
