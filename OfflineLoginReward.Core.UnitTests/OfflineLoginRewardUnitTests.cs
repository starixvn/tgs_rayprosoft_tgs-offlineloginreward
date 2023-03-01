using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.PlayerService.Client;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace OfflineLoginReward.Core.UnitTests;

public class OfflineLoginRewardUnitTests
{
	[Fact]
	public async Task 玩家登入時_未派發過彩金_要派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardCommandService(
			fakePlayerServiceClient, 
			fakePromotionQueryService,
			fakeDistributeService, 
			fakeEventService);

		var now = DateTime.UtcNow;

		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		var promotion = new OfflineLoginRewardPromotion(
			promotionId, 
			brandId,
			new EventDuring(now.AddDays(-1), now.AddDays(1)),
			new OfflineLoginRewardSettings(
				10m,
				1m,
				new HashSet<string> { playerCode }));

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());

		_ = fakeEventService.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion.PromotionId),
				Arg.Any<CancellationToken>())
			.Returns((OfflineLoginRewardEventInfo?)null);

		_ = fakePlayerServiceClient.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Enabled);

		_ = fakeEventService.IsCreateOfflineLoginRewardEventProcessingAsync(
			Arg.Is(playerCode),
			Arg.Is(promotionId),
			Arg.Any<CancellationToken>())
			.Returns(false);

		var recordId = Guid.NewGuid();

		var createEventInfo = new CreateOfflineLoginRewardEventInfo(
			playerCode,
			promotionId,
			brandId,
			10m,
			10m,
			now);

		_ = fakeEventService
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
				Arg.Any<CancellationToken>())
			.Returns(recordId);

		var command = new PlayerLoginRewardCommand(
			playerCode,
			now);

		await sut.ExecuteAsync(command);

		_ = fakeEventService
			.Received(1)
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.Received(1)
			.ExecuteAsync(
				Arg.Is(new DistributeBonusCommand(
					promotionId,
					playerCode,
					10m,
					10m,
					recordId)),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入時_若有派發彩金行為進行中_不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardCommandService(
			fakePlayerServiceClient, 
			fakePromotionQueryService,
			fakeDistributeService,
			fakeEventService);

		var now = DateTime.UtcNow;

		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		var promotion = new OfflineLoginRewardPromotion(
			promotionId,
			brandId,
			new EventDuring(now.AddDays(-1), now.AddDays(1)),
			new OfflineLoginRewardSettings(
				10m,
				1m,
				new HashSet<string> { playerCode }));

		var settings = new OfflineLoginRewardSettings(
			10m,
			1m,
			new HashSet<string> { playerCode });

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());
		
		_ = fakeEventService.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion.PromotionId),
				Arg.Any<CancellationToken>())
			.Returns((OfflineLoginRewardEventInfo?)null);

		_ = fakePlayerServiceClient.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Enabled);

		_ = fakeEventService.IsCreateOfflineLoginRewardEventProcessingAsync(
			Arg.Is(playerCode),
			Arg.Is(promotionId),
			Arg.Any<CancellationToken>())
			.Returns(true);

		var recordId = Guid.NewGuid();

		var createEventInfo = new CreateOfflineLoginRewardEventInfo(
			playerCode,
			promotionId,
			brandId,
			10m,
			10m,
			now);

		_ = fakeEventService
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
				Arg.Any<CancellationToken>())
			.Returns(recordId);

		var command = new PlayerLoginRewardCommand(
			playerCode,
			now);

		await sut.ExecuteAsync(command);

		_ = fakeEventService
			.DidNotReceive()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.ExecuteAsync(
				Arg.Is(new DistributeBonusCommand(
					promotionId,
					playerCode,
					10m,
					10m,
					recordId)),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入時_沒有在活動資格名單內_不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardCommandService(
			fakePlayerServiceClient, 
			fakePromotionQueryService,
			fakeDistributeService, 
			fakeEventService);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		var promotion = new OfflineLoginRewardPromotion(
			promotionId,
			brandId,
			new EventDuring(now.AddDays(-1), now.AddDays(1)),
			new OfflineLoginRewardSettings(
				10m,
				1m,
				new HashSet<string> { "player2" }));

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());
		
		var command = new PlayerLoginRewardCommand(
			playerCode,
			now);

		await sut.ExecuteAsync(command);

		_ = fakeEventService
			.DidNotReceive()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is<CreateOfflineLoginRewardEventInfo>(x
					=> x.PlayerCode == playerCode && x.PromotionId == promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.ExecuteAsync(
				Arg.Any<DistributeBonusCommand>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入時_找不到平台設定的活動_不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardCommandService(
			fakePlayerServiceClient,
			fakePromotionQueryService,
			fakeDistributeService,
			fakeEventService);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(Array.Empty<OfflineLoginRewardPromotion>().ToAsyncEnumerable());

		var command = new PlayerLoginRewardCommand(
			playerCode,
			now);

		await sut.ExecuteAsync(command);
		
		_ = fakeEventService
			.DidNotReceive()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is<CreateOfflineLoginRewardEventInfo>(x
					=> x.PlayerCode == playerCode && x.PromotionId == promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.ExecuteAsync(
				Arg.Any<DistributeBonusCommand>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入時_已派發過彩金_不再派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardCommandService(
			fakePlayerServiceClient, 
			fakePromotionQueryService,
 			fakeDistributeService, 
			fakeEventService);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		var promotion = new OfflineLoginRewardPromotion(
			promotionId,
			brandId,
			new EventDuring(now.AddDays(-1), now.AddDays(1)),
			new OfflineLoginRewardSettings(
				10m,
				1m,
				new HashSet<string> { "player2" }));

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

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());

		_ = fakeEventService.GetOfflineLoginRewardEventAsync(
			Arg.Is(playerCode),
			Arg.Is(promotionId),
			Arg.Any<CancellationToken>())
			.Returns(rewardEventInfo);

		var command = new PlayerLoginRewardCommand(
			playerCode, 
			now);

		await sut.ExecuteAsync(command);

		_ = fakeEventService
			.DidNotReceive()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is<CreateOfflineLoginRewardEventInfo>(x
					=> x.PlayerCode == playerCode && x.PromotionId == promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.ExecuteAsync(
				Arg.Any<DistributeBonusCommand>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 玩家登入時_若玩家狀態不是Enabled_不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardCommandService(
			fakePlayerServiceClient, 
			fakePromotionQueryService,
			fakeDistributeService,
			fakeEventService);

		var now = DateTime.UtcNow;

		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotionId = Guid.NewGuid();

		var promotion = new OfflineLoginRewardPromotion(
			promotionId,
			brandId,
			new EventDuring(now.AddDays(-1), now.AddDays(1)),
			new OfflineLoginRewardSettings(
				10m,
				1m,
				new HashSet<string> { playerCode }));

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());
		
		_ = fakeEventService.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion.PromotionId),
				Arg.Any<CancellationToken>())
			.Returns((OfflineLoginRewardEventInfo?)null);

		_ = fakePlayerServiceClient.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Disabled);

		_ = fakeEventService.IsCreateOfflineLoginRewardEventProcessingAsync(
			Arg.Is(playerCode),
			Arg.Is(promotionId),
			Arg.Any<CancellationToken>())
			.Returns(false);

		var recordId = Guid.NewGuid();

		var createEventInfo = new CreateOfflineLoginRewardEventInfo(
			playerCode,
			promotionId,
			brandId,
			10m,
			10m,
			now);

		_ = fakeEventService
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
				Arg.Any<CancellationToken>())
			.Returns(recordId);

		var command = new PlayerLoginRewardCommand(
			playerCode,
			now);

		await sut.ExecuteAsync(command);

		_ = fakeEventService
			.DidNotReceive()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is<CreateOfflineLoginRewardEventInfo>(x
					=> x.PlayerCode == playerCode && x.PromotionId == promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.ExecuteAsync(
				Arg.Any<DistributeBonusCommand>(),
				Arg.Any<CancellationToken>());
	}
}
