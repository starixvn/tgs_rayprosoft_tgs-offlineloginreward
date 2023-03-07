using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

namespace Tgs.OfflineLoginReward;

internal class PromotionListManager : IPromotionListDataStore
{
	private readonly ReplaySubject<IEnumerable<OfflineLoginRewardPromotion>> _subject = new(1);

	public async IAsyncEnumerable<OfflineLoginRewardPromotion> GetPromotionListAsync(
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var promotions = await _subject
			.ToAsyncEnumerable()
			.FirstAsync(cancellationToken)
			.ConfigureAwait(false);

		foreach (var promotion in promotions)
			yield return promotion;
	}

	public void RenewPromotionList(
		IEnumerable<OfflineLoginRewardPromotion> promotions)
		=> _subject.OnNext(promotions);
}
