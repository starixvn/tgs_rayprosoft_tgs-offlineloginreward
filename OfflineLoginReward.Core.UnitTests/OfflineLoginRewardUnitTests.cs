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
		var fakePlayerService = Substitute.For<IPlayerService>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();
		var fakePromotionSettingsQueryService =
			Substitute.For<IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePlayerService,
			fakePromotionQueryService, 
			fakePromotionSettingsQueryService,
			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;

		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var playerInfo = new PlayerInfo
		{
			BrandId = brandId,
			PlayerCode = playerCode
		};

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

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePlayerService.GetPlayerInfoAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(playerInfo);

		_ = fakePlayerService.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Enabled);

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());

		_ = fakePromotionSettingsQueryService.GetAsync(
				Arg.Is(new GetOfflineLoginRewardSettingsQuery(promotionId)),
				Arg.Any<CancellationToken>())
			.Returns(settings);

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

		var command = new PlayerLoginCommand(playerCode, now);

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository
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
	public async Task 玩家登入時_沒有在活動資格名單內_不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerService = Substitute.For<IPlayerService>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();
		var fakePromotionSettingsQueryService =
			Substitute.For<IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePlayerService,
			fakePromotionQueryService,
			fakePromotionSettingsQueryService,
			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var playerInfo = new PlayerInfo
		{
			BrandId = brandId,
			PlayerCode = playerCode
		};

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

		_ = fakePlayerService.GetPlayerInfoAsync(
			Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(playerInfo);

		_ = fakePlayerService.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Enabled);
		
		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());
		
		_ = fakePromotionSettingsQueryService.GetAsync(
				Arg.Is(new GetOfflineLoginRewardSettingsQuery(promotionId)),
				Arg.Any<CancellationToken>())
			.Returns(settings);

		var command = new PlayerLoginCommand(playerCode, now);

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository
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
		var fakePlayerService = Substitute.For<IPlayerService>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();
		var fakePromotionSettingsQueryService =
			Substitute.For<IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePlayerService, 
			fakePromotionQueryService, 
			fakePromotionSettingsQueryService,
			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var playerInfo = new PlayerInfo
		{
			BrandId = brandId,
			PlayerCode = playerCode
		};
		
		var promotionId = Guid.NewGuid();

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePlayerService.GetPlayerInfoAsync(
				Arg.Is(playerCode),
			Arg.Any<CancellationToken>())
			.Returns(playerInfo);
		
		_ = fakePlayerService.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Enabled);


		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(Array.Empty<PromotionMainInfo>().ToAsyncEnumerable());

		var command = new PlayerLoginCommand(playerCode, now);

		await sut.ExecuteAsync(command);

		_ = fakePromotionSettingsQueryService
			.DidNotReceive()
			.GetAsync(
				Arg.Is(new GetOfflineLoginRewardSettingsQuery(promotionId)),
				Arg.Any<CancellationToken>());
		
		_ = fakeTgsRepository
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
		var fakePlayerService = Substitute.For<IPlayerService>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();
		var fakePromotionSettingsQueryService =
			Substitute.For<IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePlayerService, 
			fakePromotionQueryService, 
			fakePromotionSettingsQueryService,
 			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var playerInfo = new PlayerInfo
		{
			BrandId = brandId,
			PlayerCode = playerCode
		};
		
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

		_ = fakePlayerService.GetPlayerInfoAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(playerInfo);

		_ = fakePromotionQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(new[] { promotion }.ToAsyncEnumerable());

		_ = fakePromotionSettingsQueryService.GetAsync(
				Arg.Is(new GetOfflineLoginRewardSettingsQuery(promotionId)),
				Arg.Any<CancellationToken>())
			.Returns(settings);

		_ = fakeTgsRepository.GetOfflineLoginRewardEventAsync(
			Arg.Is(playerCode),
			Arg.Is(promotionId),
			Arg.Any<CancellationToken>())
			.Returns(rewardEventInfo);

		var command = new PlayerLoginCommand(playerCode, now);

		await sut.ExecuteAsync(command);

		_ = fakeTgsRepository
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
	public async Task 玩家登入時_非有效玩家不派發彩金()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePlayerService = Substitute.For<IPlayerService>();
		var fakePromotionQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();
		var fakePromotionSettingsQueryService =
			Substitute.For<IGetService<GetOfflineLoginRewardSettingsQuery, OfflineLoginRewardSettings?>>();
		var fakeDistributeService = Substitute.For<ICommandService<DistributeBonusCommand>>();
		var fakeTgsRepository = Substitute.For<ITgsRepository>();

		var sut = new PlayerLoginCommandService(
			fakeSystemClock,
			fakePlayerService, 
			fakePromotionQueryService, 
			fakePromotionSettingsQueryService,
			fakeDistributeService,
			fakeTgsRepository);

		var now = DateTime.UtcNow;
		var brandId = Guid.NewGuid();
		var playerCode = "player1";

		var playerInfo = new PlayerInfo
		{
			BrandId = brandId,
			PlayerCode = playerCode
		};

		var promotionId = Guid.NewGuid();

		_ = fakeSystemClock.UtcNow.Returns(now);

		_ = fakePlayerService.GetPlayerInfoAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(playerInfo);

		_ = fakePlayerService.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.RegisterUndone);
		
		var command = new PlayerLoginCommand(playerCode, now);

		await sut.ExecuteAsync(command);

		_ = fakePromotionQueryService
			.DidNotReceive()
			.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>());

		_ = fakeTgsRepository
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
