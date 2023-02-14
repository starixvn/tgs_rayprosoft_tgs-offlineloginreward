using Microsoft.EntityFrameworkCore;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.Repositories;
using Tgs.OfflineLoginReward.Repositories.Entities;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddOfflineLoginRewardRepositories(
		this IServiceCollection services,
		Action<IServiceProvider, DbContextOptionsBuilder>? dbOptionsConfigure)
		=> services
			.AddDbContext<TgsContext>(dbOptionsConfigure)
			.AddTransient<ITgsRepository, TgsRepository>()
			.AddSerialNumberGenerator();
}
