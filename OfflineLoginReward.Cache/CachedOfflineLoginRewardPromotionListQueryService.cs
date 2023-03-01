using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.Cache;

internal class CachedOfflineLoginRewardPromotionListQueryService 
	: IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>
{
	private readonly IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> _sourceService;
	private readonly IMemoryCache _memoryCache;

	public CachedOfflineLoginRewardPromotionListQueryService(
		IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion> sourceService,
		IMemoryCache memoryCache)
	{
		_sourceService = sourceService ?? throw new ArgumentNullException(nameof(sourceService));
		_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
	}

	public async IAsyncEnumerable<OfflineLoginRewardPromotion> QueryAsync(OfflineLoginRewardPromotionListQuery query,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var key = "OfflineLoginReward:Promotions";
		
		var promotions = await _memoryCache.GetOrCreateAsync(key, async entry =>
			{
				entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1D);

				return await _sourceService.QueryAsync(query, cancellationToken)
					.ToArrayAsync(cancellationToken)
					.ConfigureAwait(false);
			}).ConfigureAwait(false);

		foreach(var promotion in promotions ?? Array.Empty<OfflineLoginRewardPromotion>())
			yield return promotion;
	}
}
