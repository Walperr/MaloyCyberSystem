using Microsoft.EntityFrameworkCore;
using MQTTnet.AspNetCore;
using MQTTServer.Controllers;
using MQTTServer.DataContexts;
using MQTTServer.Services;
using MQTTServer.Services.Implementation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.WebHost.UseKestrel(o =>
{
    o.ListenAnyIP(1883, l => l.UseMqtt());
    o.ListenAnyIP(5000);
});

builder.Services.AddDbContext<ApplicationDataContext>(o =>
        o.UseNpgsql(builder.Configuration.GetConnectionString("MaloyDB")));

builder.Services.AddHostedMqttServer(options => options.WithDefaultEndpoint());
builder.Services.AddMqttConnectionHandler();
builder.Services.AddConnections();
builder.Services.AddSingleton<MqttController>();
builder.Services.AddScoped<IBrokerService, BrokerService>();

var app = builder.Build();

app.UseRouting();

app.UseHttpsRedirection();

app.MapConnectionHandler<MqttConnectionHandler>("/mqtt");

app.UseMqttServer(server =>
{
    var mqttController = app.Services.GetRequiredService<MqttController>();

    server.ValidatingConnectionAsync += mqttController.ValidateConnection;
    server.ClientConnectedAsync += mqttController.OnClientConnected;
    server.ClientSubscribedTopicAsync += mqttController.OnClientSubscribed;
    server.ClientUnsubscribedTopicAsync += mqttController.OnClientUnSubscribed;
    server.InterceptingSubscriptionAsync += mqttController.ValidateSubscription;
    server.InterceptingPublishAsync += mqttController.InterceptPublish;
});

app.MapControllers();

app.Run();