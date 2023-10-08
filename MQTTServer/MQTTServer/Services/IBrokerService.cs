using MQTTnet.Server;

namespace MQTTServer.Services;

public interface IBrokerService
{
    Task InitializeUser(string userID);
    Task InitializeDevice(string deviceID);

    Task OnUserConnecting(ValidatingConnectionEventArgs e);
    Task OnDeviceConnecting(ValidatingConnectionEventArgs e);
    
    Task OnClientSubscribing(InterceptingSubscriptionEventArgs e);
    
    Task OnUserSubscribed(ClientSubscribedTopicEventArgs e);
    Task OnDeviceSubscribed(ClientSubscribedTopicEventArgs e);

    Task OnUserUnsubscribed(ClientUnsubscribedTopicEventArgs e);
    Task OnDeviceUnsubscribed(ClientUnsubscribedTopicEventArgs e);

    Task OnMessagePublishing(InterceptingPublishEventArgs e);
    Task OnMessageReceived(InterceptingPublishEventArgs e);
}