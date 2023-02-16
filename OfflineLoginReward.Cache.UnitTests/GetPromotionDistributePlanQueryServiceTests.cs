using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.OfflineLoginReward.Cache;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace OfflineLoginReward.Cache.UnitTests;

public class GetPromotionDistributePlanQueryServiceTests
{
	[Fact]
	public async Task 已快取資料()
	{
		var fakeMemoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
		var fakePromotionDistributePlanService = Substitute.For<IGetService<GetPromotionDistributePlanQuery, PromotionDistributePlanInfo>>();
		var fakeLogger = NullLogger<GetCachedPromotionDistributePlanQueryService>.Instance;

		var sut = new GetCachedPromotionDistributePlanQueryService(
			fakePromotionDistributePlanService,
			fakeMemoryCache,
			fakeLogger);

		var query = new GetPromotionDistributePlanQuery(Guid.NewGuid());

		var expected = new PromotionDistributePlanInfo(
				Id: Guid.NewGuid(),
				BrandId: Guid.NewGuid(),
				query.PromotionId,
				1000L);

		_ = fakeMemoryCache.Set($"DistributePlan:{query.PromotionId}", expected);

		var actual = await sut.GetAsync(query);

		Assert.Equal(expected, actual);

		_ = fakePromotionDistributePlanService
			.DidNotReceive()
			.GetAsync(
				Arg.Is(query),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 尚未有快取資料()
	{
		var fakeMemoryCache = new MemoryCache(new OptionsWrapper<MemoryCacheOptions>(new MemoryCacheOptions()));
		var fakePromotionDistributePlanService = Substitute.For<IGetService<GetPromotionDistributePlanQuery, PromotionDistributePlanInfo>>();
		var fakeLogger = NullLogger<GetCachedPromotionDistributePlanQueryService>.Instance;

		var sut = new GetCachedPromotionDistributePlanQueryService(
			fakePromotionDistributePlanService,
			fakeMemoryCache,
			fakeLogger);

		var query = new GetPromotionDistributePlanQuery(Guid.NewGuid());

		var expected = new PromotionDistributePlanInfo(
			Id: Guid.NewGuid(),
			BrandId: Guid.NewGuid(),
			query.PromotionId,
			1000L);

		_ = fakePromotionDistributePlanService
			.GetAsync(
				 Arg.Is(query),
				 Arg.Any<CancellationToken>())
			.Returns(expected);

		var actual = await sut.GetAsync(query);

		Assert.Equal(expected, actual);

		_ = fakePromotionDistributePlanService
			.Received(1)
			.GetAsync(
				Arg.Is(query),
				Arg.Any<CancellationToken>());
	}
}
