using System.Reflection;
using System.Text.Json;
using MQTTnet.Server;
using MQTTServer.DataContexts;
using MQTTServer.Misc;
using MQTTServer.Services;

namespace MQTTServer.Controllers;

public sealed class MqttController
{
    private readonly IServiceProvider _services;

    public MqttController(IServiceProvider services)
    {
        _services = services;
    }

    public async Task OnClientConnected(ClientConnectedEventArgs e)
    {
        await using var scope = _services.CreateAsyncScope();

        var broker = scope.ServiceProvider.GetRequiredService<IBrokerService>();

        var isUserProperty = e.UserProperties?
            .FirstOrDefault(p
                => p.Name.ToLowerInvariant() == "user")?
            .Value;

        var isUser = isUserProperty?.ToLowerInvariant() == "true";

        await (isUser
            ? broker.InitializeUser(e.ClientId)
            : broker.InitializeDevice(e.ClientId));
    }
    
    public async Task ValidateConnection(ValidatingConnectionEventArgs e)
    {
        await using var scope = _services.CreateAsyncScope();

        var broker = scope.ServiceProvider.GetRequiredService<IBrokerService>();

        var isUserProperty = e.UserProperties?
            .FirstOrDefault(p =>
                p.Name.ToLowerInvariant() == "user")?
            .Value;

        var isUser = isUserProperty?.ToLowerInvariant() == "true";

        await (isUser
            ? broker.OnUserConnecting(e)
            : broker.OnDeviceConnecting(e));
    }

    public async Task OnClientUnSubscribed(ClientUnsubscribedTopicEventArgs e)
    {
        await using var scope = _services.CreateAsyncScope();

        var broker = scope.ServiceProvider.GetRequiredService<IBrokerService>();
        var dataContext = scope.ServiceProvider.GetRequiredService<ApplicationDataContext>();

        var user = await dataContext.Users.FindAsync(e.ClientId);

        if (user is not null)
        {
            await broker.OnUserUnsubscribed(e);
            return;
        }

        var device = await dataContext.Devices.FindAsync(e.ClientId);

        if (device is not null)
            await broker.OnDeviceUnsubscribed(e);
    }

    public async Task OnClientSubscribed(ClientSubscribedTopicEventArgs e)
    {
        await using var scope = _services.CreateAsyncScope();

        var broker = scope.ServiceProvider.GetRequiredService<IBrokerService>();
        var dataContext = scope.ServiceProvider.GetRequiredService<ApplicationDataContext>();

        var user = await dataContext.Users.FindAsync(e.ClientId);

        if (user is not null)
        {
            await broker.OnUserSubscribed(e);
            return;
        }

        var device = await dataContext.Devices.FindAsync(e.ClientId);

        if (device is not null)
            await broker.OnDeviceSubscribed(e);
    }

    public async Task InterceptPublish(InterceptingPublishEventArgs e)
    {
        await using var scope = _services.CreateAsyncScope();

        var broker = scope.ServiceProvider.GetRequiredService<IBrokerService>();

        var topic = e.ApplicationMessage.Topic;

        if (topic is Topics.DATA_TOPIC or Topics.CONNECTION_KEYS_TOPIC)
        {
            await broker.OnMessageReceived(e);
        }
        else
            await broker.OnMessagePublishing(e);
    }
    public async Task ValidateSubscription(InterceptingSubscriptionEventArgs e)
    {
        await using var scope = _services.CreateAsyncScope();

        var broker = scope.ServiceProvider.GetRequiredService<IBrokerService>();

        await broker.OnClientSubscribing(e);
    }
}