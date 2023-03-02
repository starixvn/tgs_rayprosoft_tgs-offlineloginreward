using NSubstitute;
using Tgs.OfflineLoginReward;

namespace OfflineLoginReward.Core.UnitTests;

public class OfflineLoginRewardEventServiceUnitTests
{
	[Fact]
	public async Task 建立發放Event()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeStoreRepository = Substitute.For<IStoreRepository>();

		var createEventInfo = new CreateOfflineLoginRewardEventInfo(
			"player1",
			Guid.NewGuid(),
			Guid.NewGuid(),
			10m,
			30m,
			DateTime.UtcNow);

		var sut = new OfflineLoginRewardEventService(fakeTgsRepository, fakeStoreRepository);

		_ = await sut.CreateOfflineLoginRewardEventAsync(createEventInfo);

		_ = fakeTgsRepository
			.Received(1)
			.CreateOfflineLoginRewardEventAsync(
				Arg.Is(createEventInfo),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 檢查是否已派發過_若快取有紀錄會直接回傳()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeStoreRepository = Substitute.For<IStoreRepository>();
		
		var sut = new OfflineLoginRewardEventService(fakeTgsRepository, fakeStoreRepository);

		var promotionId = Guid.NewGuid();
		var playerCode = "player1";
		var promotionEndTime = DateTime.UtcNow;

		_ = fakeStoreRepository.SetEventArchivedAsync(
			Arg.Is(playerCode),
			Arg.Is(promotionId),
			Arg.Is(promotionEndTime),
			Arg.Any<CancellationToken>())
			.Returns(false);

		var actual = await sut.IsEventArchivedAsync(playerCode, promotionId, promotionEndTime);

		Assert.True(actual);

		_ = fakeTgsRepository
			.DidNotReceiveWithAnyArgs()
			.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>());
	}

	[Fact] public async Task 檢查是否已派發過_若快取沒有紀錄_會檢查DB的資料_若有紀錄會一併寫入快取()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeStoreRepository = Substitute.For<IStoreRepository>();

		var sut = new OfflineLoginRewardEventService(fakeTgsRepository, fakeStoreRepository);

		var promotionId = Guid.NewGuid();
		var playerCode = "player1";
		var promotionEndTime = DateTime.UtcNow;

		var rewardInfo = new OfflineLoginRewardEventInfo(
			Guid.NewGuid(),
			playerCode,
			promotionId,
			Guid.NewGuid(),
			10m,
			30m,
			DateTime.UtcNow,
			DateTime.UtcNow);

		_ = fakeStoreRepository.SetEventArchivedAsync(
				Arg.Is(playerCode),
				Arg.Is(promotionId),
				Arg.Is(promotionEndTime),
				Arg.Any<CancellationToken>())
			.Returns(true);

		_ = fakeTgsRepository.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>())
			.Returns(rewardInfo);

		var actual = await sut.IsEventArchivedAsync(playerCode, promotionId, promotionEndTime);

		Assert.True(actual);

		_ = fakeTgsRepository
			.Received(1)
			.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>());

		_ = fakeStoreRepository
			.Received(2)
			.SetEventArchivedAsync(
				Arg.Is(playerCode),
				Arg.Is(promotionId),
				Arg.Is(promotionEndTime),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 檢查是否已派發過_若快取沒有紀錄_會檢查DB的資料_且DB沒有紀錄()
	{
		var fakeTgsRepository = Substitute.For<ITgsRepository>();
		var fakeStoreRepository = Substitute.For<IStoreRepository>();

		var sut = new OfflineLoginRewardEventService(fakeTgsRepository, fakeStoreRepository);

		var promotionId = Guid.NewGuid();
		var playerCode = "player1";
		var promotionEndTime = DateTime.UtcNow;
		
		_ = fakeStoreRepository.SetEventArchivedAsync(
				Arg.Is(playerCode),
				Arg.Is(promotionId),
				Arg.Is(promotionEndTime),
				Arg.Any<CancellationToken>())
			.Returns(true);

		_ = fakeTgsRepository.GetOfflineLoginRewardEventAsync(
				Arg.Is(playerCode),
				Arg.Is(promotionId),
				Arg.Any<CancellationToken>())
			.Returns((OfflineLoginRewardEventInfo?)null);

		var actual = await sut.IsEventArchivedAsync(playerCode, promotionId, promotionEndTime);

		Assert.False(actual); ;
	}
}