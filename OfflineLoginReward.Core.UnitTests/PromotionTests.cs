using ExpectedObjects;
using NSubstitute;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

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

	[Fact]
	public async Task 取得符合條件的登入送活動()
	{
		var fakeSystemClock = Substitute.For<ISystemClock>();
		var fakePromotionListQueryService =
			Substitute.For<IQueryService<OfflineLoginRewardPromotionListQuery, PromotionMainInfo>>();

		var brandId = Guid.NewGuid();
		var utcNow = DateTime.UtcNow;

		var promotions = new PromotionMainInfo[]
		{
			new()
			{
				Id = Guid.NewGuid(),
				PromotionType = "OfflineLoginReward",
				BrandId = brandId,
				EventDuring = new During(utcNow.AddDays(-1D), utcNow.AddDays(1D)),
				Name = "TestOfflineLoginReward1",
			},
			new()
			{
				Id = Guid.NewGuid(),
				PromotionType = "OfflineLoginReward",
				BrandId = Guid.NewGuid(),
				EventDuring = new During(utcNow.AddDays(-2D), utcNow.AddDays(-1D)),
				Name = "TestOfflineLoginReward2",
			},
			new()
			{
				Id = Guid.NewGuid(),
				PromotionType = "OfflineLoginReward",
				BrandId = brandId,
				EventDuring = new During(utcNow.AddDays(-4D), utcNow.AddDays(-2D)),
				Name = "TestOfflineLoginReward3",
			}
		};

		_ = fakeSystemClock.UtcNow.Returns(utcNow);

		_ = fakePromotionListQueryService.QueryAsync(
				Arg.Is(new OfflineLoginRewardPromotionListQuery()),
				Arg.Any<CancellationToken>())
			.Returns(promotions.ToAsyncEnumerable());

		var query = new QualifiedOfflineLoginRewardPromotionListQuery(brandId);

		var sut = new QualifiedOfflineLoginRewardPromotionListQueryService(
			fakeSystemClock,
			fakePromotionListQueryService);

		var actual = await sut.QueryAsync(query).ToArrayAsync();

		Assert.NotNull(actual);
		_ = Assert.Single(actual);
	}
}