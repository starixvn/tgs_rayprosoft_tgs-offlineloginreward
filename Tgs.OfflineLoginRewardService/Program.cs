using Microsoft.AspNetCore.Builder;
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
		options.Url = builder.Configuration.GetValue<Uri>("OfflineLoginReward:PromotionBaseUrl")!);

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
				"tgs.operator.player.login", 
				"tgs.operator.player.login"))
	.ClearAllExchanges()
	.AddNatsGlobPatternExchange("*");

builder.Services
	.AddRouting(options => options.LowercaseUrls = true)
	.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();