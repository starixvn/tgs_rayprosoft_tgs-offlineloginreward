using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.OfflineLoginReward.Cache;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineLoginRewardCache(this IServiceCollection services)
		=> services
			.Decorate<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>,
				CachedOfflineLoginRewardPromotionListQueryService>()
			.Decorate<IGetService<GetPlayerInfoQuery, PlayerInfo?>, GetCachedPlayerInfoQueryService>();
}