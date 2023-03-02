using FluentdForward.OpenTelemetry.MessagePack;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tgs.OfflineLoginReward;
using Tgs.OfflineLoginReward.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true)
	.AddEnvironmentVariables();

builder.Logging
	.AddOpenTelemetry(options =>
	{
		options.IncludeFormattedMessage = true;
		options.ParseStateValues = true;
		_ = options.AddFluentdForwardExporter(fluentOptions =>
		{
			var nodeIp = builder.Configuration.GetValue<string>("NODE_IP")!;
			fluentOptions.Host = nodeIp;
			fluentOptions.Tag = builder.Configuration.GetValue<string>("OTEL_SERVICE_NAME")!;
			fluentOptions.UseMessagePack(LogRecordFormatterResolver.GetResolverInstanceWithExtendInfo(
				"MachineInfo",
				() => new
				{
					Environment.MachineName
				}));
		});
	});

builder.Services
	.AddOfflineLoginRewardCore((options, _) =>
	{
		var section = builder.Configuration.GetSection("PromotionListRefresh");
		options.PeriodBySeconds = section.GetValue<int>("PeriodBySeconds");
	})
	.AddSingleton(sp =>
	{
		var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
		return ActivatorUtilities.CreateInstance<RedisConnectionManager>(sp, redisConnectionString);
	})
	.AddOfflineLoginRewardRepositories(
		(_, contextBuilder) => contextBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Tgs")!),
		sp =>
		{
			var redisConnectionManager = sp.GetRequiredService<RedisConnectionManager>();
			return redisConnectionManager.Database;
		})
	.AddMemoryCache()
	.AddOfflineLoginRewardCache()
	.AddPromotionClientServices((options, _) =>
		options.Url = builder.Configuration.GetValue<Uri>("OfflineLoginReward:PromotionBaseUrl")!)
	.AddPlayerServiceClient((options, _) =>
		options.BaseUrl = builder.Configuration.GetValue<Uri>("OfflineLoginReward:PlayerServiceBaseUrl")!);

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig =>
		natsConfig
			.ConfigQueueOptions((options, _) =>
				options.Url = builder.Configuration.GetConnectionString("Nats")!)
			.AddProcessor<OfflineLoginRewardSettingProcessor>(
				"tgs.promotion.offlineloginreward.setting",
				"tgs.promotion.offlineloginreward.setting")
			.AddHandler<PlayerLoginEventHandler>(
				"tgs.operator.player.logged_in",
				"tgs.operator.player.logged_in"))
	.ClearAllExchanges()
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddOpenTelemetry()
	.ConfigureResource(_ => ResourceBuilder.CreateDefault()
		.AddEnvironmentVariableDetector())
	.WithTracing(
		tracingBuilder => tracingBuilder
			.AddSource("Tgs.*")
			.AddSource("TGS.*")
			.AddNpgsql()
			.AddAspNetCoreInstrumentation()
			.AddEntityFrameworkCoreInstrumentation(options => options.SetDbStatementForText = true)
			.AddProfileViewExporter()
			.AddOtlpExporter())
	.WithMetrics(meterBuilder => meterBuilder
		.AddHttpClientInstrumentation()
		.AddRuntimeInstrumentation()
		.AddAspNetCoreInstrumentation()
	)
	.Services
	.AddProfileViewer()
	.Configure<AspNetCoreInstrumentationOptions>(options => options.Filter =
		httpContext => !httpContext.Request.Path.HasValue 
			|| !httpContext.Request.Path.StartsWithSegments("/swagger")
			&& !httpContext.Request.Path.StartsWithSegments("/healthz")
		    && !httpContext.Request.Path.StartsWithSegments("/metrics")
		    && !httpContext.Request.Path.StartsWithSegments("/profiler/")
		    && !new[] { "html", "ico", "png", "jpg" }.Any(
				ext => httpContext.Request.Path.Value.EndsWith($".{ext}")));

builder.Services
	.AddInterceptionService()
	.AddServiceUtiltiyInterceptors();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthz");
app.MapProfileViewer();

app.Run();
