using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

builder.Services.AddMassTransit(x =>
{
  x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

  x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

  x.UsingRabbitMq((context, config) =>
  {
    config.ReceiveEndpoint("search-auction-created", error =>
    {
      error.UseMessageRetry(r => r.Interval(5, 5));
      error.ConfigureConsumer<AuctionCreatedConsumer>(context);
    });

    config.ConfigureEndpoints(context); ;
  });
});

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{

  try
  {
    await DBInitializer.InitDb(app);
  }
  catch (Exception e)
  {
    Console.WriteLine(e);
  }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
  => HttpPolicyExtensions.HandleTransientHttpError()
  .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
  .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
