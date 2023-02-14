using ExpectedObjects;
using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.Promotion.Clients;

namespace OfflineLoginReward.Core.UnitTests;

public class PromotionServiceTests
{
	[Fact]
	public async Task 取得登入送活動()
	{
		var fakePromotionServiceClient = Substitute.For<IPromotionServiceClient>();

		var brandId = Guid.NewGuid();
		var now = DateTime.UtcNow;
		
		var promotions = new PromotionMainInfo[]
		{
			new()
			{
				Id = Guid.NewGuid(),
				PromotionType = "OfflineLoginReward",
				BrandId = brandId,
				EventDuring = new During(now.AddDays(-1D), now.AddDays(1D)),
				Name = "TestOfflineLoginReward1",
			},
			new()
			{
				Id = Guid.NewGuid(),
				PromotionType = "OfflineLoginReward",
				BrandId = Guid.NewGuid(),
				EventDuring = new During(now.AddDays(-2D), now.AddDays(-1D)),
				Name = "TestOfflineLoginReward2",
			},
			new()
			{
				Id = Guid.NewGuid(),
				PromotionType = "OfflineLoginReward",
				BrandId = brandId,
				EventDuring = new During(now.AddDays(-4D), now.AddDays(-2D)),
				Name = "TestOfflineLoginReward3",
			}
		};

		_ = fakePromotionServiceClient.ListPromotionsByPromotionTypeAsync(
				Arg.Is("OfflineLoginReward"),
				Arg.Any<CancellationToken>())
			.Returns(promotions.ToAsyncEnumerable());
		
		var sut = new PromotionService(fakePromotionServiceClient);

		var actual = await sut
			.GetOfflineLoginRewardPromotionsAsync()
			.ToArrayAsync();

		Assert.NotNull(actual);
		Assert.Equal(3, actual.Length);
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

		_ = fakePromotionServiceClient.GetPromotionRuleAsync<PromotionService.PromotionRule>(
				Arg.Is(promotionId))
			.Returns(new PromotionService.PromotionRule
			{
				CampaignDetail = settings
			});
		
		var sut = new PromotionService(fakePromotionServiceClient);

		var expected = new OfflineLoginRewardSettings(
			10m,
			1m,
			new[] { "player1", "player2" });

		var actual = await sut.GetOfflineLoginRewardSettingsAsync(promotionId);

		Assert.NotNull(actual);

		expected.ToExpectedObject(ctx 
				=> ctx.IgnoreEqualsOverride())
			.ShouldEqual(actual);
	}
}