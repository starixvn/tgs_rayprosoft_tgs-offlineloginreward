using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.Promotion.Clients;

namespace OfflineLoginReward.Core.UnitTests;

public class DistributeServiceTests
{
	[Fact]
	public async Task 取得派發計畫()
	{
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

		var promotionId = Guid.NewGuid();

		var sut = new DistributeService(fakePromotionServiceClient);

		_ = await sut.GetDistributePlanInfoAsync(promotionId);

		_ = fakePromotionServiceClient
			.Received(1)
			.GetPromotionDistributePlanAsync(
				Arg.Is(promotionId),
				Arg.Is(1000L),
				Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 派發彩金()
	{
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

		var planId = Guid.NewGuid();
		var playerCode = "player1";
		var bonusAmount = 10m;
		var bucketAmount = 30m;
		var rewardId = Guid.NewGuid();

		var sut = new DistributeService(fakePromotionServiceClient);

		_ = await sut.DistributeBonusAsync(planId, playerCode, bonusAmount, bucketAmount, rewardId);

		_ = fakePromotionServiceClient
			.Received(1)
			.DistributePromotionBonusAsync(
				Arg.Is(planId),
				Arg.Is(playerCode),
				Arg.Is(bonusAmount),
				Arg.Is(bucketAmount),
				Arg.Is((string?)null),
				Arg.Is(rewardId),
				Arg.Any<CancellationToken>());
	}
}