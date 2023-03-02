using System.Reflection;
using Tgs.OfflineLoginReward;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineLoginRewardCore(
		this IServiceCollection services,
		Action<PromotionListRefreshOptions, IServiceProvider> configure)
		=> services
			.AddOptions<PromotionListRefreshOptions>()
			.Configure(configure)
			.Services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddTransient<IOfflineLoginRewardEventService, OfflineLoginRewardEventService>()
			.AddTransient<IPlayerLoginRewardHandler, PlayerLoginRewardHandler>()
			.AddSingleton<OfflineLoginRewardPromotionListManager>()
			.AddTransient<IOfflineLoginRewardPromotionListStore>(sp => sp.GetRequiredService<OfflineLoginRewardPromotionListManager>())
			.AddHostedService<OfflineLoginRewardPromotionListManagerBackgroundService>();
}