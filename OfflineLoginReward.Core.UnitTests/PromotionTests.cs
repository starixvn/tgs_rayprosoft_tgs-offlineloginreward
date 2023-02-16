using ExpectedObjects;
using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Promotion.Clients;

namespace OfflineLoginReward.Core.UnitTests;

public class PromotionTests
{
	[Fact]
	public async Task 取得登入送活動()
	{
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();
		
		var query = new OfflineLoginRewardPromotionListQuery();
		var sut = new OfflineLoginRewardPromotionListQueryService(fakePromotionServiceClient);

		_ = await sut
			.QueryAsync(query)
			.ToArrayAsync();

		_ = fakePromotionServiceClient
			.Received(1)
			.ListPromotionsByPromotionTypeAsync(
				Arg.Is("OfflineLoginReward"),
			Arg.Any<CancellationToken>());
	}

	[Fact]
	public async Task 取得登入送CampaignDetail規則()
	{
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

		var promotionId = Guid.NewGuid();

		var settings = new OfflineLoginRewardSettings(
			10m,
			1m,
			new[] { "player1", "player2" });

		_ = fakePromotionServiceClient.GetPromotionRuleAsync<GetOfflineLoginRewardSettingsQueryService.PromotionRule>(
				Arg.Is(promotionId))
			.Returns(new GetOfflineLoginRewardSettingsQueryService.PromotionRule
			{
				CampaignDetail = settings
			});

		var query = new GetOfflineLoginRewardSettingsQuery(promotionId);
		var sut = new GetOfflineLoginRewardSettingsQueryService(fakePromotionServiceClient);

		var expected = new OfflineLoginRewardSettings(
			10m,
			1m,
			new[] { "player1", "player2" });

		var actual = await sut.GetAsync(query);

		Assert.NotNull(actual);

		expected.ToExpectedObject(ctx 
				=> ctx.IgnoreEqualsOverride())
			.ShouldEqual(actual);
	}
}