using System.Text.Json;
using Google.Protobuf;
using Tgs.MessageQueue;
using Tgs.Promotion.Clients.Protos;

namespace Tgs.OfflineLoginReward.MessageQueue;

internal class OfflineLoginRewardSettingProcessor : IMessageProcessor
{
	public ValueTask<ReadOnlyMemory<byte>> HandleAsync(
		ReadOnlyMemory<byte> data,
		CancellationToken cancellationToken = default)
	{
		var request = CampaignDetailRequest.Parser.ParseFrom(data.Span);

		var settings = JsonSerializer.Deserialize<OfflineLoginRewardSettings>(
			request.Settings, 
			OfflineLoginRewardUtility.SerializerOptions);

		if (settings == null)
			throw new InvalidOperationException("Settings is null");

		if (settings.BonusAmount <= 0m)
			throw new InvalidOperationException("BonusAmount should be greater than 0");

		if (settings.TurnoverMultiple <= 0)
			throw new InvalidOperationException("TurnoverMultiple should be greater than 0");

		if (!settings.PlayerCodes.Any())
			throw new InvalidOperationException("Missing PlayerCodes");

		var response = new CampaignDetailResponse
		{
			CampaignDetail = JsonSerializer.Serialize(settings, OfflineLoginRewardUtility.SerializerOptions),
			TurnoverMultiple = settings.TurnoverMultiple
		};

		return ValueTask.FromResult((ReadOnlyMemory<byte>) response.ToByteArray());
	}
}
