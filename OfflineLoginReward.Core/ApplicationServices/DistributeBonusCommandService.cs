using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tgs.Promotion.Clients;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward.ApplicationServices;

internal class DistributeBonusCommandService : ICommandService<DistributeBonusCommand>
{
	private readonly IPromotionServiceClient _promotionServiceClient;
	private readonly IGetService<GetPromotionDistributePlanQuery, PromotionDistributePlanInfo> _getDistributePlanService;

	public DistributeBonusCommandService(
		IPromotionServiceClient promotionServiceClient,
		IGetService<GetPromotionDistributePlanQuery, PromotionDistributePlanInfo> getDistributePlanService)
	{
		_promotionServiceClient = promotionServiceClient ?? throw new ArgumentNullException(nameof(promotionServiceClient));
		_getDistributePlanService = getDistributePlanService ?? throw new ArgumentNullException(nameof(getDistributePlanService));
	}

	public async ValueTask ExecuteAsync(
		DistributeBonusCommand command, 
		CancellationToken cancellationToken = default)
	{
		var plan = await _getDistributePlanService
			.GetAsync(new(command.PromotionId), cancellationToken)
			.ConfigureAwait(false);

		_ = await _promotionServiceClient.DistributePromotionBonusAsync(
			plan.Id,
			command.PlayerCode,
			command.BonusAmount,
			command.BucketAmount,
			null,
			command.RewardRecordId,
			cancellationToken)
			.ConfigureAwait(false);
	}
}