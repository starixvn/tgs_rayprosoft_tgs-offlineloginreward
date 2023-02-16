using NSubstitute;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.PlayerService.Client;

namespace OfflineLoginReward.Core.UnitTests;

public class PlayerTests
{
	[Fact]
	public async Task 取得玩家資料()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();

		var playerCode = "player1";
		var query = new GetPlayerInfoQuery(playerCode);

		var sut = new GetPlayerInfoQueryService(fakePlayerServiceClient);

		_ = await sut.GetAsync(query);

		_ = fakePlayerServiceClient
			.Received(1)
			.GetPlayerInfoAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>());
	}
}
