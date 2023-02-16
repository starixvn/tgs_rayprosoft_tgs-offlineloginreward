using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace OfflineLoginReward.Core.UnitTests;

public class DistributeBonusTests
{
	[Fact]
	public async Task 取得派發計畫()
	{
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

		var promotionId = Guid.NewGuid();

		var query = new GetPromotionDistributePlanQuery(promotionId);
		var sut = new GetPromotionDistributePlanQueryService(fakePromotionServiceClient);

		_ = await sut.GetAsync(query);

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
		var fakeDistributePlanQueryService = Substitute
			.For<IGetService<GetPromotionDistributePlanQuery, PromotionDistributePlanInfo>>();

		var promotionId = Guid.NewGuid();
		var playerCode = "player1";
		var bonusAmount = 10m;
		var bucketAmount = 30m;
		var rewardId = Guid.NewGuid();

		var distributePlanInfo = new PromotionDistributePlanInfo(
			Guid.NewGuid(),
			Guid.NewGuid(),
			promotionId,
			1000L);

		_ = fakeDistributePlanQueryService.GetAsync(
				Arg.Is(new GetPromotionDistributePlanQuery(promotionId)),
				Arg.Any<CancellationToken>())
			.Returns(distributePlanInfo);

		var command = new DistributeBonusCommand(
			promotionId,
			playerCode,
			bonusAmount,
			bucketAmount,
			rewardId);

		var sut = new DistributeBonusCommandService(fakePromotionServiceClient, fakeDistributePlanQueryService);

		await sut.ExecuteAsync(command);

		_ = fakePromotionServiceClient
			.Received(1)
			.DistributePromotionBonusAsync(
				Arg.Is(distributePlanInfo.Id),
				Arg.Is(playerCode),
				Arg.Is(bonusAmount),
				Arg.Is(bucketAmount),
				Arg.Is((string?)null),
				Arg.Is(rewardId),
				Arg.Any<CancellationToken>());
	}
}