using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tgs.OfflineLoginReward;

public static class OfflineLoginRewardUtility
{
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		WriteIndented = false,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
	};
}
