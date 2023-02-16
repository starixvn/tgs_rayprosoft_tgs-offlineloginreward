using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class GetPromotionDistributePlanQueryService : IGetService<GetPromotionDistributePlanQuery, PromotionDistributePlanInfo>
{
	private readonly IPromotionServiceClient _promotionServiceClient;
	private static readonly long _ConditionHash = 1000L;

	public GetPromotionDistributePlanQueryService(
		IPromotionServiceClient promotionServiceClient)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
	}

	public ValueTask<PromotionDistributePlanInfo> GetAsync(GetPromotionDistributePlanQuery query,
		CancellationToken cancellationToken = default) 
		=> _promotionServiceClient.GetPromotionDistributePlanAsync(
			query.PromotionId, 
			_ConditionHash, 
			cancellationToken);
}
