using FlowStream.Connect.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<ISqsMessageReceiver, InMemorySqsMessageReceiver>();
builder.Services.AddSingleton<IKivraClient, FakeKivraClient>();
builder.Services.AddHostedService<DocumentDispatchWorker>();

var host = builder.Build();
host.Run();
