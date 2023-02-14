using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Promotion.Clients;

namespace OfflineLoginReward.Core.UnitTests;

public class OfflineLoginRewardUnitTests
{
	[Fact]
	public async Task 玩家登入並派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePromotionService = Substitute.For<IPromotionService>();
		var fakeDistributeService = Substitute.For<IDistributeService>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePromotionService,
			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		var promotion = new PromotionMainInfo
		{
			Id = promotionId,
			EventDuring = new During(now.AddDays(-1), now.AddDays(1)),
			Name = "TestPromotion",
			PromotionType = "OfflineLoginReward",
			BrandId = brandId,
			IsAutoSent = true
		};

		var settings = new OfflineLoginRewardSettings(
			10m, 
			1m, 
			new[] { playerCode });

		var distributePlan = new PromotionDistributePlanInfo(
			Guid.NewGuid(),
			brandId, 
			promotionId,
			1000L);

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionService.GetOfflineLoginRewardPromotionsAsync(
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());

		_ = fakePromotionService.GetOfflineLoginRewardSettingsAsync(
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>())
			.Returns(settings);

		_ = fakeDistributeService.GetDistributePlanInfoAsync(
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>())
			.Returns(distributePlan);

		var recordId = Guid.NewGuid();

		_ = fakeTgsRepository
			.CreateOfflineLoginRewardRecordAsync(
				Arg.Is(promotionId),
				Arg.Is(playerCode),
				Arg.Is(10m),
				Arg.Is(10m),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>())
			.Returns(recordId);

		var command = new PlayerLoginCommand(brandId, playerCode);

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository
			.Received(1)
			.CreateOfflineLoginRewardRecordAsync(
				Arg.Is(promotionId),
				Arg.Is(playerCode),
				Arg.Is(10m),
				Arg.Is(10m),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.Received(1)
			.DistributeBonusAsync(
				Arg.Is(distributePlan.Id),
				Arg.Is(playerCode),
				Arg.Is(10m),
				Arg.Is(10m),
				Arg.Is(recordId),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入後但沒有符合資格將不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePromotionService = Substitute.For<IPromotionService>();
		var fakeDistributeService = Substitute.For<IDistributeService>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePromotionService,
			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		var promotion = new PromotionMainInfo
		{
			Id = promotionId,
			EventDuring = new During(now.AddDays(-1), now.AddDays(1)),
			Name = "TestPromotion",
			PromotionType = "OfflineLoginReward",
			BrandId = brandId,
			IsAutoSent = true
		};

		var settings = new OfflineLoginRewardSettings(
			10m,
			1m,
			new[] { "player2" });

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionService.GetOfflineLoginRewardPromotionsAsync(
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());

		_ = fakePromotionService.GetOfflineLoginRewardSettingsAsync(
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>())
			.Returns(settings);

		var command = new PlayerLoginCommand(brandId, playerCode);

		await sut.ExecuteAsync(command);

		_ = fakeDistributeService
			.DidNotReceive()
			.GetDistributePlanInfoAsync(
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeTgsRepository
			.DidNotReceive()
			.CreateOfflineLoginRewardRecordAsync(
				Arg.Is(promotionId),
				Arg.Is(playerCode),
				Arg.Any<decimal>(),
				Arg.Any<decimal>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.DistributeBonusAsync(
				Arg.Any<Guid>(),
				Arg.Is(playerCode),
				Arg.Is(10m),
				Arg.Is(10m),
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入後但沒有符合設定的活動將不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePromotionService = Substitute.For<IPromotionService>();
		var fakeDistributeService = Substitute.For<IDistributeService>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePromotionService,
			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionService.GetOfflineLoginRewardPromotionsAsync(
				Arg.Any<CancellationToken>())
			.Returns(Array.Empty<PromotionMainInfo>().ToAsyncEnumerable());

		var command = new PlayerLoginCommand(brandId, playerCode);

		await sut.ExecuteAsync(command);

		_ = fakeDistributeService
			.DidNotReceive()
			.GetDistributePlanInfoAsync(
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeTgsRepository
			.DidNotReceive()
			.CreateOfflineLoginRewardRecordAsync(
				Arg.Is(promotionId),
				Arg.Is(playerCode),
				Arg.Any<decimal>(),
				Arg.Any<decimal>(),
				Arg.Any<DateTime>(),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.DistributeBonusAsync(
				Arg.Any<Guid>(),
				Arg.Is(playerCode),
				Arg.Is(10m),
				Arg.Is(10m),
				Arg.Any<Guid>(),
				Arg.Any<CancellationToken>());
	}
}
