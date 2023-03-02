using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.ServiceUtility;

namespace OfflineLoginReward.Core.UnitTests;

public class OfflineLoginRewardPromotionListManagerUnitTests
{
	[Fact]
	public async Task Manager在初始化時會預先更新PromotionList()
	{
		var options = Options.Create(new PromotionListRefreshOptions()
		{
			PeriodBySeconds = 300
		});
		var fakeLogger = NullLogger<OfflineLoginRewardPromotionListManager>.Instance;
		var fakePromotionListQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();
		
		var services = new ServiceCollection();
		services.AddSingleton(fakeLogger);
		services.AddSingleton(fakePromotionListQueryService);

		var sut = new OfflineLoginRewardPromotionListManager(
			services.BuildServiceProvider(),
			options,
			fakeLogger);

		await sut.InitAsync();

		_ = fakePromotionListQueryService
			.Received(1)
			.QueryAsync(
				Arg.Any<OfflineLoginRewardPromotionListQuery>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task Manager在初始化後會定時更新PromotionList()
	{
		var options = Options.Create(new PromotionListRefreshOptions
		{
			PeriodBySeconds = 2
		});
		var fakeLogger = NullLogger<OfflineLoginRewardPromotionListManager>.Instance;
		var fakePromotionListQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();

		var services = new ServiceCollection();
		services.AddSingleton(fakeLogger);
		services.AddSingleton(fakePromotionListQueryService);

		var sut = new OfflineLoginRewardPromotionListManager(
			services.BuildServiceProvider(),
			options,
			fakeLogger);

		await sut.InitAsync();

		await Task.Delay(3000);

		sut.OnStop();

		_ = fakePromotionListQueryService
			.Received(2)
			.QueryAsync(
				Arg.Any<OfflineLoginRewardPromotionListQuery>(),
				Arg.Any<CancellationToken>());
	}

}
