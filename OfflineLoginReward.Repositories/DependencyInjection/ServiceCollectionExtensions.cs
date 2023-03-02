using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.Repositories;
using Tgs.OfflineLoginReward.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineLoginRewardRepositories(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder>? dbOptionsConfigure,
		Func<IServiceProvider, IDatabase> redisDatabaseResolver)
		=> services
			.AddDbContext<TgsContext>(dbOptionsConfigure)
			.AddTransient<ITgsRepository, TgsRepository>()
			.AddTransient<IStoreRepository>(sp 
				=> ActivatorUtilities.CreateInstance<RedisStoreRepository>(sp, redisDatabaseResolver(sp)) )
			.AddSerialNumberGenerator();
}
