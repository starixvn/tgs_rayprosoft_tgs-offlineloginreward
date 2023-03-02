using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.OfflineLoginReward.Cache;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineLoginRewardCache(this IServiceCollection services)
		=> services
			.Decorate<IGetService<GetPromotionDistributePlanQuery, PromotionDistributePlanInfo>,
				GetCachedPromotionDistributePlanQueryService>();
}