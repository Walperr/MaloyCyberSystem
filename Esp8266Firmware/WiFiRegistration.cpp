#include "WiFiRegistration.h"

static DNSServer _dnsServer;
static ESP8266WebServer _webServer(80);

const char ConnectionPage[] PROGMEM = R"rawliteral(
<!DOCTYPE HTML><html><head>
<meta name="viewport" content="width=device-width, initial-scale=1">
</head><body>
<style type="text/css">
    input[type="text"] {margin-bottom:8px;font-size:20px;}
    input[type="password"] {margin-bottom:8px;font-size:20px;}
    input[type="number"] {margin-bottom:8px;font-size:20px;}
    input[type="submit"] {width:180px; height:60px;margin-bottom:8px;font-size:20px;}
</style>
<center>
<h3>Maloy Device SN001</h3>
<form action="/connect" method="POST">
    <h3>WiFi network configurations</h3>
    <input type="text" name="ssid" placeholder="WiFi SSID">
    <input type="password" name="pass" placeholder="Password">
    <h3>Broker server configurations</h3>
    <input type="text" name="brokerIp" placeholder="Broker IP">
    <input type="number" name="port" placeholder="Port">
    <input type="submit" value="Configure">
</form>
</center>
</body></html>)rawliteral";

static bool _started = false;
static byte _status = 0;

Configuration configuration;

void OnConnect()
{
    strcpy(configuration.WiFiSSID, _webServer.arg("ssid").c_str());
    strcpy(configuration.Password, _webServer.arg("pass").c_str());
    strcpy(configuration.BrokerAddress, _webServer.arg("brokerIp").c_str());

    uint16_t  port;

    sscanf(_webServer.arg("port").c_str(), "%d", &port);
    configuration.Port = port;

    _status = 1;
}

void OnExit()
{
    _status = 4;
}

void StartServer()
{
    WiFi.softAPdisconnect();
    WiFi.disconnect();
    IPAddress ip(IP);
    IPAddress subnet(255,255,255,0);
    WiFi.mode(WIFI_AP);
    WiFi.softAPConfig(ip, ip, subnet);
    WiFi.softAP(configuration.NetworkName);
    _dnsServer.start(53, "*", ip);

    _webServer.onNotFound([]() 
    {
        _webServer.send(200, "text/html", ConnectionPage);
    });
    _webServer.on("/connect", HTTP_POST, OnConnect);
    _webServer.on("/exit", HTTP_POST, OnExit);
    _webServer.begin();
    _started = true;
    _status = 0;
}

void StopServer()
{
    WiFi.softAPdisconnect();
    _webServer.stop();
    _dnsServer.stop();
    _started = false;
}

bool ServerTick()
{
    if (_started)
    {
        _dnsServer.processNextRequest();
        _webServer.handleClient();
        yield();
        if (_status)
        {
            StopServer();
            return true;
        }
    }

    return false;
}

void RunServer(uint32_t timeout)
{
    uint32_t now = millis();
    StartServer();
    while(!ServerTick());

    yield();
}

byte ServerStatus()
{
    return _status;
}