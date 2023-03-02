using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.OfflineLoginReward.ApplicationServices;
using Tgs.ServiceUtility;

namespace Tgs.OfflineLoginReward;

internal class OfflineLoginRewardPromotionListManager : IOfflineLoginRewardPromotionListStore, IDisposable
{
	private readonly IServiceProvider _serviceProvider;
	private readonly ILogger<OfflineLoginRewardPromotionListManager> _logger;

	private readonly TimeSpan _promotionListRefreshPeriod;
	private PeriodicTimer? _timer;
	private CancellationTokenSource? _cancellationTokenSource;
	private bool _disposedValue;
	
	private bool _isStarted = false;
	private Task? _backgroundTask;

	private OfflineLoginRewardPromotion[]? _promotions;

	public OfflineLoginRewardPromotionListManager(
		IServiceProvider serviceProvider,
		IOptions<PromotionListRefreshOptions> promotionListRefreshOptions,
		ILogger<OfflineLoginRewardPromotionListManager> logger)
	{
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		_promotionListRefreshPeriod = TimeSpan.FromSeconds(promotionListRefreshOptions.Value.PeriodBySeconds);
	}

	public async ValueTask InitAsync(CancellationToken cancellationToken = default)
	{
		if (_isStarted)
			return;

		_cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		
		await UpdateOfflineLoginRewardPromotionListAsync(_cancellationTokenSource.Token)
			.ConfigureAwait(false);

		_timer = new PeriodicTimer(_promotionListRefreshPeriod);
		
		_backgroundTask = Task.Run(async () =>
		{
			while (await _timer.WaitForNextTickAsync(_cancellationTokenSource.Token))
			{
				if (_cancellationTokenSource.Token.IsCancellationRequested)
					break;

				_logger.LogDebug("Time: {time}", DateTime.Now);

				try
				{
					await UpdateOfflineLoginRewardPromotionListAsync(_cancellationTokenSource.Token)
						.ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Fail to update offline-login-reward promotion list");
				}
			}
		}, cancellationToken);

		_isStarted = true;
	}

	public void OnStop()
	{
		if (_cancellationTokenSource?.IsCancellationRequested == false)
			_cancellationTokenSource?.Cancel();

		if (_backgroundTask?.IsCompleted == false)
			_backgroundTask?.Wait();

		_timer?.Dispose();
		_backgroundTask?.Dispose();
		_cancellationTokenSource?.Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private async ValueTask UpdateOfflineLoginRewardPromotionListAsync(CancellationToken cancellationToken = default)
	{
		await using var scope = _serviceProvider.CreateAsyncScope();

		var queryService = scope.ServiceProvider
			.GetRequiredService<IQueryService<OfflineLoginRewardPromotionListQuery, OfflineLoginRewardPromotion>>();

		_promotions = await queryService
			.QueryAsync(new OfflineLoginRewardPromotionListQuery(), cancellationToken)
			.ToArrayAsync(cancellationToken)
			.ConfigureAwait(false);
	}

	public IAsyncEnumerable<OfflineLoginRewardPromotion> QueryOfflineLoginRewardPromotionListAsync(
		CancellationToken cancellationToken = default)
		=> (_promotions ?? Array.Empty<OfflineLoginRewardPromotion>()).ToAsyncEnumerable();

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposedValue)
		{
			if (disposing)
			{
				if (_cancellationTokenSource?.IsCancellationRequested == false)
					_cancellationTokenSource?.Cancel();

				_timer?.Dispose();
				_cancellationTokenSource?.Dispose();
			}

			_disposedValue = true;
		}
	}
}
