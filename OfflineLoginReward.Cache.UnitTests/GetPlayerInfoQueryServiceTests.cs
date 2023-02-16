using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.OfflineLoginReward.Cache;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace OfflineLoginReward.Cache.UnitTests;

public class GetPlayerInfoQueryServiceTests
{
	[Fact]
	public async Task 已快取資料()
	{
		var fakeMemoryCache = new MemoryCache(
			new OptionsWrapper<MemoryCacheOptions>(
				new MemoryCacheOptions()));

		var fakeSourceService =
			Substitute.For<IGetService<GetPlayerInfoQuery, PlayerInfo?>>();

		var playerCode = "player1";

		var expected = new PlayerInfo
			{
				PlayerCode = playerCode,
				BrandId = Guid.NewGuid()
			};

		_ = fakeMemoryCache.Set($"PlayerInfo:{playerCode}", expected);

		var sut = new GetCachedPlayerInfoQueryService(
			fakeSourceService,
			fakeMemoryCache);

		var query = new GetPlayerInfoQuery(playerCode);

		var actual = await sut.GetAsync(query);

		Assert.Equal(expected, actual);

		_ = fakeSourceService
			.DidNotReceive()
			.GetAsync(
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
			Substitute.For<IGetService<GetPlayerInfoQuery, PlayerInfo?>>();

		var playerCode = "player1";

		var expected = new PlayerInfo
		{
			PlayerCode = playerCode,
			BrandId = Guid.NewGuid()
		};

		var query = new GetPlayerInfoQuery(playerCode);

		_ = fakeSourceService.GetAsync(
				Arg.Is(query),
				Arg.Any<CancellationToken>())
			.Returns(expected);

		var sut = new GetCachedPlayerInfoQueryService(
			fakeSourceService,
			fakeMemoryCache);

		var actual = await sut.GetAsync(query);

		Assert.Equal(expected, actual);

		_ = fakeSourceService
			.Received(1)
			.GetAsync(
				Arg.Is(query),
				Arg.Any<CancellationToken>());
	}
}
