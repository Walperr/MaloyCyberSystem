using System.Globalization;
using System.Text.RegularExpressions;
using MQTTnet.Protocol;
using MQTTnet.Server;
using MQTTServer.DataContexts;
using MQTTServer.Misc;
using MQTTServer.Models;
using Server = MQTTnet.Server.MqttServer;

namespace MQTTServer.Services.Implementation;

internal sealed class BrokerService : IBrokerService
{
    private readonly Regex _commandsRegex = new("Device/Commands/[\\w#+]+$", RegexOptions.Singleline);
    private readonly ApplicationDataContext _dataContext;
    private readonly Regex _notificationRegex = new("Device/Notification/[\\w#+]+$", RegexOptions.Singleline);
    private readonly Server _server;

    public BrokerService(ApplicationDataContext dataContext, Server server)
    {
        _dataContext = dataContext;
        _server = server;
    }

    public async Task InitializeUser(string userID)
    {
        var user = await _dataContext.Users.FindAsync(userID);

        if (user is null)
        {
            await _server.DisconnectClientAsync(userID, MqttDisconnectReasonCode.NotAuthorized);
            return;
        }

        var tasks = from topic in user.SubscribedTopics ?? Enumerable.Empty<string>()
            select _server.SubscribeAsync(userID, topic);

        await Task.WhenAll(tasks);
    }

    public async Task InitializeDevice(string deviceID)
    {
        var device = await _dataContext.Devices.FindAsync(deviceID);

        if (device is null)
        {
            await _server.DisconnectClientAsync(deviceID, MqttDisconnectReasonCode.NotAuthorized);
            return;
        }

        var tasks = from topic in device.SubscribedTopics ?? Enumerable.Empty<string>()
            select _server.SubscribeAsync(deviceID, topic);

        await Task.WhenAll(tasks);
    }

    public Task OnUserConnecting(ValidatingConnectionEventArgs e)
    {
        var registerProperty = e.UserProperties?.FirstOrDefault(p => p.Name.ToLowerInvariant() == "register");
        var isRegisterRequired = registerProperty?.Value?.ToLowerInvariant() == "true";

        return isRegisterRequired
            ? RegisterUser(e)
            : AuthorizeUser(e);
    }

    public async Task OnDeviceConnecting(ValidatingConnectionEventArgs e)
    {
        var deviceID = e.ClientId;
        var salt = Guid.NewGuid().ToString();
        string passwordHash;

        var device = await _dataContext.FindAsync<Device>(deviceID);

        if (device is null)
        {
            passwordHash = e.Password.GetPasswordHash(salt);

            device = new Device
            {
                DeviceID = deviceID,
                DeviceName = e.UserName,
                PasswordHash = passwordHash,
                Salt = salt
            };

            await _dataContext.Devices.AddAsync(device);
            await _dataContext.SaveChangesAsync();

            await _server.InjectApplicationMessage(Topics.SERVER_NOTIFICATIONS_TOPIC, "Devices updated");

            return;
        }

        salt = device.Salt;

        passwordHash = e.Password.GetPasswordHash(salt);

        if (device.PasswordHash != passwordHash)
            e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
    }

    public async Task OnClientSubscribing(InterceptingSubscriptionEventArgs e)
    {
        var topic = e.TopicFilter.Topic;

        switch (topic)
        {
            case Topics.DATA_TOPIC:
            case Topics.CONNECTION_KEYS_TOPIC:
            case Topics.FLAT_DEVICE_WILDCARD:
            case Topics.RECURSIVE_DEVICE_WILDCARD:
                e.Response.ReasonCode = MqttSubscribeReasonCode.TopicFilterInvalid;
                e.ProcessSubscription = false;
                return;
        }

        if (_notificationRegex.IsMatch(topic))
        {
            var deviceID = topic.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];

            var user = await _dataContext.Users.FindAsync(e.ClientId);

            if (user is null)
            {
                e.Response.ReasonCode = MqttSubscribeReasonCode.NotAuthorized;
                e.ProcessSubscription = false;
                return;
            }

            if (!user.ConnectedDevicesID?.Contains(deviceID) ?? true)
            {
                e.Response.ReasonCode = MqttSubscribeReasonCode.TopicFilterInvalid;
                e.ProcessSubscription = false;
            }
        }

        if (_commandsRegex.IsMatch(topic))
        {
            var deviceID = topic.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];

            var device = await _dataContext.Devices.FindAsync(deviceID);

            if (device is null || deviceID != e.ClientId)
            {
                e.Response.ReasonCode = MqttSubscribeReasonCode.TopicFilterInvalid;
                e.ProcessSubscription = false;
            }
        }
    }

    public async Task OnUserSubscribed(ClientSubscribedTopicEventArgs e)
    {
        var user = await _dataContext.Users.FindAsync(e.ClientId);

        if (user is null)
            return;

        var subscribedTopics = user.SubscribedTopics ??= new List<string>();

        if (!subscribedTopics.Contains(e.TopicFilter.Topic))
            subscribedTopics.Add(e.TopicFilter.Topic);

        await _dataContext.SaveChangesAsync();
    }

    public async Task OnDeviceSubscribed(ClientSubscribedTopicEventArgs e)
    {
        var device = await _dataContext.Devices.FindAsync(e.ClientId);

        if (device is null)
            return;

        var subscribedTopics = device.SubscribedTopics ??= new List<string>();

        if (!subscribedTopics.Contains(e.TopicFilter.Topic))
            subscribedTopics.Add(e.TopicFilter.Topic);

        await _dataContext.SaveChangesAsync();
    }

    public async Task OnUserUnsubscribed(ClientUnsubscribedTopicEventArgs e)
    {
        var user = await _dataContext.Users.FindAsync(e.ClientId);

        if (user is null)
            return;

        user.SubscribedTopics?.Remove(e.TopicFilter);

        await _dataContext.SaveChangesAsync();
    }

    public async Task OnDeviceUnsubscribed(ClientUnsubscribedTopicEventArgs e)
    {
        var device = await _dataContext.Devices.FindAsync(e.ClientId);

        if (device is null)
            return;

        device.SubscribedTopics?.Remove(e.TopicFilter);

        await _dataContext.SaveChangesAsync();
    }

    public async Task OnMessagePublishing(InterceptingPublishEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;

        if (!_commandsRegex.IsMatch(topic))
            return;

        var user = await _dataContext.Users.FindAsync(e.ClientId);

        if (user is null)
        {
            e.ProcessPublish = false;
            e.Response.ReasonCode = MqttPubAckReasonCode.NotAuthorized;
            return;
        }

        var deviceID = topic.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];

        if (!user.ConnectedDevicesID?.Contains(deviceID) ?? true)
        {
            e.ProcessPublish = false;
            e.Response.ReasonCode = MqttPubAckReasonCode.TopicNameInvalid;
        }
    }

    public async Task OnMessageReceived(InterceptingPublishEventArgs e)
    {
        if (e.ApplicationMessage.Topic is Topics.CONNECTION_KEYS_TOPIC)
        {
            var isUserProperty = e.ApplicationMessage.UserProperties?
                .FirstOrDefault(p
                    => p.Name.ToLowerInvariant() == "user")?
                .Value;

            var payload = e.ApplicationMessage.PayloadSegment.ConvertToString();
            if (payload.Length != 4)
                return;

            var code = Convert.ToInt32(payload);

            var isUser = isUserProperty?.ToLowerInvariant() == "true";

            if (!isUser)
                await StoreConnectionKey(e.ClientId, code);
            else
                await TryConnectUserAndDevice(e, code);
        }

        if (e.ApplicationMessage.Topic is Topics.DATA_TOPIC)
        {
            var payload = e.ApplicationMessage.PayloadSegment.ConvertToString();

            var split = payload.Split('_', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            try
            {
                var timeUnix = ulong.Parse(split[0]);
                var value = double.Parse(split[1].Replace(',', ','), CultureInfo.InvariantCulture);

                var device = await _dataContext.Devices.FindAsync(e.ClientId);

                if (device is null)
                    return;

                device.AddValue(timeUnix, value);

                await _dataContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private async Task AuthorizeUser(ValidatingConnectionEventArgs e)
    {
        var user = await _dataContext.Users.FindAsync(e.ClientId);

        if (user is null)
        {
            e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
            return;
        }

        var passwordHash = e.Password.GetPasswordHash(user.Salt);

        if (user.PasswordHash != passwordHash)
            e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
    }

    private async Task RegisterUser(ValidatingConnectionEventArgs e)
    {
        var salt = Guid.NewGuid().ToString();

        var user = await _dataContext.Users.FindAsync(e.ClientId);

        if (user is not null)
        {
            e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
            return;
        }

        user = new User
        {
            UserID = e.ClientId,
            Salt = salt,
            Username = e.UserName,
            PasswordHash = e.Password.GetPasswordHash(salt)
        };

        await _dataContext.Users.AddAsync(user);

        await _dataContext.SaveChangesAsync();
    }

    private async Task TryConnectUserAndDevice(InterceptingPublishEventArgs e, int code)
    {
        var deviceID = e.ApplicationMessage.UserProperties?
            .FirstOrDefault(p =>
                p.Name == "deviceID")?
            .Value ?? string.Empty;

        var device = await _dataContext.Devices.FindAsync(deviceID);

        if (device is null || device.ConnectionCode == -1 || device.ConnectionCode != code)
            return;

        var user = await _dataContext.Users.FindAsync(e.ClientId);

        if (user is null)
            return;

        var connectedDevices = user.ConnectedDevicesID ??= new List<string>();

        if (!connectedDevices.Contains(deviceID))
            connectedDevices.Add(deviceID);

        device.ConnectionCode = -1;

        await _dataContext.SaveChangesAsync();
        await _server.SubscribeAsync(e.ClientId, $"{Topics.NOTIFICATIONS_TOPIC}{deviceID}");

        await _server.InjectApplicationMessage(Topics.SERVER_NOTIFICATIONS_TOPIC,
            $"Connected device and user {e.ClientId}");

        await _server.InjectApplicationMessage($"{Topics.COMMANDS_TOPIC}{deviceID}", $"print/connected {e.ClientId}");
    }

    private async Task StoreConnectionKey(string deviceID, int code)
    {
        var device = await _dataContext.Devices.FindAsync(deviceID);

        if (device is null)
            return;

        device.ConnectionCode = code;

        await _dataContext.SaveChangesAsync();
    }
}