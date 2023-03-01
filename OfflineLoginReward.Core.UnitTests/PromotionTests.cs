using NSubstitute;
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
}