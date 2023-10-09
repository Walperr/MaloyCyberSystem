#ifndef _WiFIRegistration_h
#define _WiFIRegistration_h
#endif

#define IP 192,168,1,1

#include <DNSServer.h>
#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>

#define WiFi_ERROR 0
#define WiFi_SUBMIT 1
#define WifI_EXIT 2

struct Configuration
{
    String NetworkName = "";
    char WiFiSSID[32] = "";
    char Password[32] = "";
    char BrokerAddress[32] = "localhost";
    uint16_t Port = 1883;
};

extern Configuration configuration;

void RunServer(uint32_t timeout = 4294697295);
byte ServerStatus();