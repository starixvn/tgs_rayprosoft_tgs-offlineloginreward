using System.Reflection;
using Tgs.OfflineLoginReward;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineLoginRewardCore(
		this IServiceCollection services,
		Action<PromotionListRenewOptions, IServiceProvider> configure)
		=> services
			.AddOptions<PromotionListRenewOptions>()
			.Configure(configure)
			.Services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddTransient<IOfflineLoginRewardEventService, OfflineLoginRewardEventService>()
			.AddTransient<IPlayerLoginRewardHandler, PlayerLoginRewardHandler>()
			.AddSingleton<PromotionListDataStore>()
			.AddHostedService<PromotionListManagerBackgroundService>();
}