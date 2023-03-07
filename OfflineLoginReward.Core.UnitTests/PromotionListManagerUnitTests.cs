using Tgs.OfflineLoginReward;

namespace OfflineLoginReward.Core.UnitTests;

public class PromotionListManagerUnitTests
{
	[Fact]
	public async Task 如果沒有更新PromotionList會等待取得資料()
	{
		// Arrange
		var sut = new PromotionListManager();

		// Act
		var actual = await Assert.ThrowsAsync<TimeoutException>(
			async () => await sut.GetPromotionListAsync().ToArrayAsync()
									.AsTask().WaitAsync(TimeSpan.FromSeconds(1D)));

		// Assert
		Assert.NotNull(actual);
	}

	[Fact]
	public async Task 取得PromotionList會得到最後更新的結果()
	{
		// Arrange
		var sut = new PromotionListManager();

		sut.RenewPromotionList(Array.Empty<OfflineLoginRewardPromotion>());
		sut.RenewPromotionList(new[]
		{
			new OfflineLoginRewardPromotion(
				Guid.NewGuid(),
				Guid.NewGuid(),
				new EventDuring(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(1)),
				new OfflineLoginRewardSettings(
					10m,
					1m,
					new HashSet<string> { "player1" }))
		});

		// Act
		var actual = await sut.GetPromotionListAsync().ToArrayAsync();

		// Assert
		_ = Assert.Single(actual);
	}
}
