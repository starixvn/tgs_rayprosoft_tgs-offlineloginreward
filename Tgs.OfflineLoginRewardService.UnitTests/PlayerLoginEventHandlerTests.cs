using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NSubstitute;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.OfflineLoginReward.MessageQueue;
using Tgs.Operator.Client.Protos;
using Tgs.PlayerService.Client;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginRewardService.UnitTests;

public class PlayerLoginEventHandlerTests
{
	[Fact]
	public async Task 收到登入事件後會進行活動判斷處理()
	{
		var fakePlayerServiceClient = Substitute.For<IPlayerServiceClient>();
		var fakeLoginRewardCommand = Substitute.For<ICommandService<PlayerLoginRewardCommand>>();

		var sut = new PlayerLoginEventHandler(fakeLoginRewardCommand);

		var playerCode = "player1";
		var loggedInOn = DateTime.UtcNow;
		
		_ = fakePlayerServiceClient.GetPlayerStatusAsync(
				Arg.Is(playerCode),
				Arg.Any<CancellationToken>())
			.Returns(PlayerStatus.Enabled);

		var loggedInEvent = new LoggedInEvent
		{
			PlayerCode = playerCode,
			LoggedInTime = loggedInOn.ToTimestamp()
		};

		await sut.HandleAsync(loggedInEvent.ToByteArray());

		_ = fakeLoginRewardCommand
			.Received(1)
			.ExecuteAsync(
				Arg.Is(
					new PlayerLoginRewardCommand(
						playerCode,
						loggedInOn)),
				Arg.Any<CancellationToken>());
	}
}
