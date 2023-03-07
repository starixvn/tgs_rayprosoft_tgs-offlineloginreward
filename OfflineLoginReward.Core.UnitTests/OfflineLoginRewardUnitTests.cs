using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;

namespace OfflineLoginReward.Core.UnitTests;

public class OfflineLoginRewardUnitTests
{
	[Fact]
	public async Task 玩家登入時_且符合資格時_會進行派發後續處理()
	{
		var fakePromotionDataStore = Substitute.For<IPromotionListDataStore>();
		var fakePlayerLoginRewardHandler = Substitute.For<IPlayerLoginRewardHandler>();
		var fakeLogger = NullLogger<PlayerLoginRewardCommandService>.Instance;

		var sut = new PlayerLoginRewardCommandService(
			fakePromotionDataStore, 
			fakePlayerLoginRewardHandler);
		
		var brandId = Guid.NewGuid();
		var playerCode = "player1";
		
		var now = DateTime.UtcNow;

		var promotion = new OfflineLoginRewardPromotion(
			Guid.NewGuid(), 
			brandId,
			new EventDuring(now.AddDays(-1), now.AddDays(1)),
			new OfflineLoginRewardSettings(
				10m,
				1m,
				new HashSet<string> { playerCode }));

		_ = fakePromotionDataStore.GetPromotionListAsync(
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());
		
		var command = new PlayerLoginRewardCommand(
			playerCode,
			now);

		await sut.ExecuteAsync(command);

		_ = fakePlayerLoginRewardHandler
			.Received(1)
			.OnDistributeRewardAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入時_找不到平台設定的活動_不做後續處理()
	{
		var fakePromotionDataStore = Substitute.For<IPromotionListDataStore>();
		var fakePlayerLoginRewardHandler = Substitute.For<IPlayerLoginRewardHandler>();
		var fakeLogger = NullLogger<PlayerLoginRewardCommandService>.Instance;

		var sut = new PlayerLoginRewardCommandService(
			fakePromotionDataStore,
			fakePlayerLoginRewardHandler);

		var now = DateTime.UtcNow;
		var playerCode = "player1";

		_ = fakePromotionDataStore.GetPromotionListAsync(
				Arg.Any<CancellationToken>())
			.Returns(Array.Empty<OfflineLoginRewardPromotion>().ToAsyncEnumerable());

		var command = new PlayerLoginRewardCommand(
			playerCode,
			now);

		await sut.ExecuteAsync(command);

		_ = fakePlayerLoginRewardHandler
			.DidNotReceiveWithAnyArgs()
			.OnDistributeRewardAsync(
				Arg.Is(playerCode),
				Arg.Any<OfflineLoginRewardPromotion>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>());
	}
}

