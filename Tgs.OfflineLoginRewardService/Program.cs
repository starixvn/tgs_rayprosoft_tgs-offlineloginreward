using Microsoft.EntityFrameworkCore;
using Tgs.OfflineLoginReward.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true)
	.AddEnvironmentVariables();

builder.Logging
	.AddConsole();

builder.Services
	.AddOfflineLoginRewardCore()
	.AddOfflineLoginRewardRepositories((_, contextBuilder) 
		=> contextBuilder.UseNpgsql(builder.Configuration.GetConnectionString("Tgs")!))
	.AddPromotionClientServices((options, _) =>
		options.Url = builder.Configuration.GetValue<Uri>("OfflineLoginReward:PromotionBaseUrl")!)
	.AddPlayerServiceClient((options, _) =>
		options.BaseUrl = builder.Configuration.GetValue<Uri>("OfflineLoginReward:PlayerServiceBaseUrl")!);

builder.Services
	.AddMemoryCache()
	.AddOfflineLoginRewardCache();

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig =>
		natsConfig
			.ConfigQueueOptions((options, _) =>
				options.Url = builder.Configuration.GetConnectionString("Nats")!)
			.AddProcessor<OfflineLoginRewardSettingProcessor>(
				"tgs.promotion.offlineloginreward.setting",
				"tgs.promotion.offlineloginreward.setting")
			.AddHandler<PlayerLoginHandler>(
				"tgs.operator.player.logged_in",
				"tgs.operator.player.logged_in"))
	.ClearAllExchanges()
	.AddNatsGlobPatternExchange("*");

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthz");

app.Run();