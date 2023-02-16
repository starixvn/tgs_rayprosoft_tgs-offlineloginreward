using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.OfflineLoginReward.Cache;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace OfflineLoginReward.Cache.UnitTests;

public class OfflineLoginRewardPromotionListQueryServiceTests
{
	[Fact]
	public async Task 已快取資料()
	{
		var fakeMemoryCache = new MemoryCache(
			new OptionsWrapper<MemoryCacheOptions>(
				new MemoryCacheOptions()));

		var fakeSourceService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();

		var expected = new[]
		{
			new PromotionMainInfo
			{
				Id = Guid.NewGuid(),
				BrandId = Guid.NewGuid(),
				Name = "TestPromotion1",
				EventDuring = new During(
					DateTime.UtcNow.AddDays(-1),
					DateTime.UtcNow.AddDays(1)),
				PromotionType = "OfflineLoginReward"
			},
			new PromotionMainInfo
			{
				Id = Guid.NewGuid(),
				BrandId = Guid.NewGuid(),
				Name = "TestPromotion2",
				EventDuring = new During(
					DateTime.UtcNow.AddDays(-3),
					DateTime.UtcNow.AddDays(2)),
				PromotionType = "OfflineLoginReward"
			}
		};

		_ = fakeMemoryCache.Set("OfflineLoginReward:Promotions", expected);

		var sut = new CachedOfflineLoginRewardPromotionListQueryService(
			fakeSourceService,
			fakeMemoryCache);

		var query = new OfflineLoginRewardPromotionListQuery();

		var actual = await sut.QueryAsync(query).ToArrayAsync();

		Assert.Equal(expected, actual);

		_ = fakeSourceService
			.DidNotReceive()
			.QueryAsync(
				Arg.Is(query),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 尚未有快取資料()
	{
		var fakeMemoryCache = new MemoryCache(
			new OptionsWrapper<MemoryCacheOptions>(
				new MemoryCacheOptions()));

		var fakeSourceService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();

		var expected = new[]
		{
			new PromotionMainInfo
			{
				Id = Guid.NewGuid(),
				BrandId = Guid.NewGuid(),
				Name = "TestPromotion1",
				EventDuring = new During(
					DateTime.UtcNow.AddDays(-1),
					DateTime.UtcNow.AddDays(1)),
				PromotionType = "OfflineLoginReward"
			},
			new PromotionMainInfo
			{
				Id = Guid.NewGuid(),
				BrandId = Guid.NewGuid(),
				Name = "TestPromotion2",
				EventDuring = new During(
					DateTime.UtcNow.AddDays(-3),
					DateTime.UtcNow.AddDays(2)),
				PromotionType = "OfflineLoginReward"
			}
		};

		var query = new OfflineLoginRewardPromotionListQuery();

		_ = fakeSourceService.QueryAsync(
					Arg.Is(query),
					Arg.Any<CancellationToken>())
			.Returns(expected.ToAsyncEnumerable());

		var sut = new CachedOfflineLoginRewardPromotionListQueryService(
			fakeSourceService,
			fakeMemoryCache);

		var actual = await sut.QueryAsync(query).ToArrayAsync();

		Assert.Equal(expected, actual);

		_ = fakeSourceService
			.Received(1)
			.QueryAsync(
				Arg.Is(query),
				Arg.Any<CancellationToken>());
	}
}
