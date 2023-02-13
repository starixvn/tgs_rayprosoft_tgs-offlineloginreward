using System.Text.Json;
using ExpectedObjects;
using Google.Protobuf;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.MessageQueue;
using Tgs.Promotion.Clients.Protos;

namespace Tgs.OfflineLoginRewardService.UnitTests;

public class OfflineLoginRewardSettingProcessorTests
{
	[Fact]
	public async Task 處理Setting請求並回傳()
	{
		var settings = new 
		{
			BonusAmount = 10m,
			TurnoverMultiple = 1m, 
			PlayerCodes = new[] {"player1", "player2"}
		};

		var request = new CampaignDetailRequest
		{
			PromotionId = Guid.NewGuid(),
			Settings = JsonSerializer.Serialize(settings)
		};

		var sut = new OfflineLoginRewardSettingProcessor();

		var expected = new OfflineLoginRewardSettings(
			10,
			1,
			new List<string> { "player1", "player2" });

		var response = await sut.HandleAsync(request.ToByteArray());

		var actual = CampaignDetailResponse.Parser.ParseFrom(response.Span);

		var actualSettings =
			JsonSerializer.Deserialize<OfflineLoginRewardSettings>(actual.CampaignDetail, OfflineLoginRewardUtility.SerializerOptions);

		Assert.NotNull(actualSettings);

		expected
			.ToExpectedObject(ctx => ctx.IgnoreEqualsOverride())
			.ShouldMatch(actualSettings);
	}

	[Fact]
	public async Task Setting的BonusAmount未大於零會丟出錯誤()
	{
		var settings = new
		{
			BonusAmount = -10m,
			TurnoverMultiple = 1m,
			PlayerCodes = new[] { "player1", "player2" }
		};

		var request = new CampaignDetailRequest
		{
			PromotionId = Guid.NewGuid(),
			Settings = JsonSerializer.Serialize(settings)
		};

		var sut = new OfflineLoginRewardSettingProcessor();

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(async() => 
			await sut.HandleAsync(request.ToByteArray()));
			
		Assert.Contains("BonusAmount", exception.Message);
	}

	[Fact]
	public async Task Setting的TurnoverMultiple未大於零會丟出錯誤()
	{
		var settings = new
		{
			BonusAmount = 10m,
			TurnoverMultiple = -1m,
			PlayerCodes = new[] { "player1", "player2" }
		};

		var request = new CampaignDetailRequest
		{
			PromotionId = Guid.NewGuid(),
			Settings = JsonSerializer.Serialize(settings)
		};

		var sut = new OfflineLoginRewardSettingProcessor();

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await sut.HandleAsync(request.ToByteArray()));

		Assert.Contains("TurnoverMultiple", exception.Message);
	}

	[Fact]
	public async Task Setting若沒有傳入PlayerCodes會丟出錯誤()
	{
		var settings = new
		{
			BonusAmount = 10m,
			TurnoverMultiple = 1m,
			PlayerCodes = Array.Empty<string>()
		};

		var request = new CampaignDetailRequest
		{
			PromotionId = Guid.NewGuid(),
			Settings = JsonSerializer.Serialize(settings)
		};

		var sut = new OfflineLoginRewardSettingProcessor();

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
			await sut.HandleAsync(request.ToByteArray()));

		Assert.Contains("PlayerCodes", exception.Message);
	}
}