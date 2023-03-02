using System.Reflection;
using Tgs.OfflineLoginReward;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineLoginRewardCore(this IServiceCollection services)
		=> services
			.RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
			.AddTransient<IOfflineLoginRewardEventService, OfflineLoginRewardEventService>()
			.AddTransient<IPlayerLoginRewardHandler, PlayerLoginRewardHandler>();
}