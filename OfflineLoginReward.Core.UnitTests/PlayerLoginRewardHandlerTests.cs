using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace OfflineLoginReward.Core.UnitTests;

public class PlayerLoginRewardHandlerTests
{
	[Fact]
	public async Task 處理派發時_若未派發過彩金_進行派發()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardHandler(
			fakePlayerServiceClient,
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

		_ = fakeEventService.IsEventArchivedAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion.PromotionId),
				Arg.Is(promotion.EventDuring.EndTime),
				Arg.Any<CancellationToken>())
			.Returns(false);

		_ = fakePlayerServiceClient.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Enabled);

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

		await sut.OnDistributeRewardAsync(playerCode, promotion, now);

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
	public async Task 處理派發時_已派發過彩金_不進行派發()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardHandler(
			fakePlayerServiceClient,
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
				new HashSet<string> { "player1" }));

		_ = fakeEventService.IsEventArchivedAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion.PromotionId),
				Arg.Is(promotion.EventDuring.EndTime),
				Arg.Any<CancellationToken>())
			.Returns(true);

		await sut.OnDistributeRewardAsync(playerCode, promotion, now);

		_ = fakeEventService
			.DidNotReceiveWithAnyArgs()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Any<CreateOfflineLoginRewardEventInfo>(),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceiveWithAnyArgs()
			.ExecuteAsync(
				Arg.Any<DistributeBonusCommand>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 處理派發時_若玩家狀態不是Enabled_不進行派發()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardHandler(
			fakePlayerServiceClient,
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

		_ = fakeEventService.IsEventArchivedAsync(
				Arg.Is(playerCode),
				Arg.Is(promotion.PromotionId),
				Arg.Is(promotion.EventDuring.EndTime),
				Arg.Any<CancellationToken>())
			.Returns(false);

		_ = fakePlayerServiceClient.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Disabled);

		await sut.OnDistributeRewardAsync(playerCode, promotion, now);

		_ = fakeEventService
			.DidNotReceiveWithAnyArgs()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Any<CreateOfflineLoginRewardEventInfo>(),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.ExecuteAsync(
				Arg.Any<DistributeBonusCommand>(),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 處理派發時_沒有在活動設定名單內_不進行派發()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeEventService = Substitute.For<IOfflineLoginRewardEventService>();

		var sut = new PlayerLoginRewardHandler(
			fakePlayerServiceClient,
			fakeDistributeService,
			fakeEventService);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var promotion = new OfflineLoginRewardPromotion(
			Guid.NewGuid(),
			brandId,
			new EventDuring(now.AddDays(-1), now.AddDays(1)),
			new OfflineLoginRewardSettings(
				10m,
				1m,
				new HashSet<string> { "player2" }));

		await sut.OnDistributeRewardAsync(playerCode, promotion, now);

		_ = fakeEventService
			.DidNotReceiveWithAnyArgs()
			.CreateOfflineLoginRewardEventAsync(
				Arg.Any<CreateOfflineLoginRewardEventInfo>(),
				Arg.Any<CancellationToken>());

		_ = fakeDistributeService
			.DidNotReceive()
			.ExecuteAsync(
				Arg.Any<DistributeBonusCommand>(),
				Arg.Any<CancellationToken>());
	}
}
