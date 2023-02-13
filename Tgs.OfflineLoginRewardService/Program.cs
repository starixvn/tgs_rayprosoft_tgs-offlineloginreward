using Tgs.OfflineLoginReward.MessageQueue;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
	.AddJsonFile("/config/appsettings.json", true)
	.AddEnvironmentVariables();

builder.Logging
	.AddConsole();

builder.Services
	.AddOfflineLoginRewardCore();

builder.Services
	.AddMessageQueue()
	.AddNatsMessageQueue(natsConfig =>
		natsConfig
			.ConfigQueueOptions((options, _) =>
				options.Url = builder.Configuration.GetConnectionString("Nats")!)
			.AddProcessor<OfflineLoginRewardSettingProcessor>(
				"tgs.promotion.offlineloginreward.setting",
				"tgs.promotion.offlineloginreward.setting"))
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