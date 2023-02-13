using System.Text.Json;
using Google.Protobuf;
using Tgs.MessageQueue;
using Tgs.Promotion.Clients.Protos;

namespace Tgs.OfflineLoginReward.MessageQueue;

public class OfflineLoginRewardSettingProcessor : IMessageProcessor
{
	private static readonly JsonSerializerOptions _SerializerOptions = new()
	{
		WriteIndented = false,
		PropertyNameCaseInsensitive = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public ValueTask<ReadOnlyMemory<byte>> HandleAsync(
		ReadOnlyMemory<byte> data,
		CancellationToken cancellationToken = default)
	{
		var request = CampaignDetailRequest.Parser.ParseFrom(data.Span);

		var settings = JsonSerializer.Deserialize<OfflineLoginRewardSettings>(request.Settings);

		if (settings == null)
			throw new InvalidOperationException("Settings is null");

		if (settings.BonusAmount <= 0m)
			throw new InvalidOperationException("BonusAmount should be greater than 0");

		if (settings.PlayerCodes.Length == 0)
			throw new InvalidOperationException("Missing PlayerCodes");

		var response = new CampaignDetailResponse
		{
			CampaignDetail = JsonSerializer.Serialize(settings, _SerializerOptions),
			TurnoverMultiple = settings.TurnoverMultiple
		};

		return ValueTask.FromResult((ReadOnlyMemory<byte>) response.ToByteArray());
	}
}
