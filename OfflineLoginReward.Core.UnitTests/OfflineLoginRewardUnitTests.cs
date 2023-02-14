using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Promotion.Clients;

namespace OfflineLoginReward.Core.UnitTests;

public class OfflineLoginRewardUnitTests
{
	[Fact]
	public async Task 玩家登入且未派發過彩金時要派發彩金()
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

		_ = fakeTgsRepository.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion.Id),
				Arg.Any<CancellationToken>())
			.Returns((OfflineLoginRewardEventInfo?)null);

		var recordId = Guid.NewGuid();

		var createEventInfo = new CreateOfflineLoginRewardEventInfo(
			playerCode,
			promotionId,
			brandId,
			10m,
			10m,
			now);

		_ = fakeTgsRepository
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
				Arg.Any<CancellationToken>())
			.Returns(recordId);

		var command = new PlayerLoginCommand(brandId, playerCode);

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository
			.Received(1)
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
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
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is<CreateOfflineLoginRewardEventInfo>(x 
					=> x.PlayerCode == playerCode && x.PromotionId == promotionId),
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
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is<CreateOfflineLoginRewardEventInfo>(x
					=> x.PlayerCode == playerCode && x.PromotionId == promotionId),
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
	public async Task 玩家登入且已派發過彩金將不再派發彩金()
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

		var rewardEventInfo = new OfflineLoginRewardEventInfo(
			Guid.NewGuid(),
			playerCode,
			promotionId,
			brandId,
			10m,
			10m,
			now,
			now);

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionService.GetOfflineLoginRewardPromotionsAsync(
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());

		_ = fakePromotionService.GetOfflineLoginRewardSettingsAsync(
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>())
			.Returns(settings);

		_ = fakeTgsRepository.GetOfflineLoginRewardEventAsync(
			Arg.Is(playerCode),
			Arg.Is(promotionId),
			Arg.Any<CancellationToken>())
			.Returns(rewardEventInfo);

		var command = new PlayerLoginCommand(brandId, playerCode);

		await sut.ExecuteAsync(command);

		_ = fakeDistributeService
			.DidNotReceive()
			.GetDistributePlanInfoAsync(
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeTgsRepository
			.DidNotReceive()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is<CreateOfflineLoginRewardEventInfo>(x
					=> x.PlayerCode == playerCode && x.PromotionId == promotionId),
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
