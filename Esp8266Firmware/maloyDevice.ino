#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <LiquidCrystal.h>
#include <time.h>
#include <sntp.h>
#include <TZ.h>
#include "WiFiRegistration.h"

#define MYTZ TZ_Asia_Novosibirsk
#define SERIAL_NUMBER "SN001"
#define DEVICE_PASSWORD "AONGEhjgefjcs3299jJOJNONHFce9FvGFEBn2"
#define COMMANDS_TOPIC "Device/Commands/" SERIAL_NUMBER
#define CONNECTIONS_TOPIC "Device/Connections/" SERIAL_NUMBER
#define NOTIFICATION_TOPIC "Device/Notification/" SERIAL_NUMBER

#define CLEAN_LINE "                "

WiFiClient espClient;
PubSubClient client(espClient);

const uint rs = 12, enable = 13, d4 = 0, d5 = 4, d6 = 5, d7 = 16;

LiquidCrystal lcd(rs, enable, d4, d5, d6, d7);

String ConvertTimeToString(time_t time)
{
  tm *ltm = localtime(&time);

  int year = ltm->tm_year + 1900;
  int month = ltm->tm_mon + 1;
  int day = ltm->tm_mday;
  int hour = ltm->tm_hour;
  int min = ltm->tm_min;
  int sec = ltm->tm_sec;

  return String(year)+"-"+ToTwoDigitString(month)+"-"+ToTwoDigitString(day)+"T"+ToTwoDigitString(hour)+":"+ToTwoDigitString(min)+":"+ToTwoDigitString(sec)+"Z";
}

String ToTwoDigitString(int value)
{
  String str = "00";
  str[0] = '0' + value / 10;
  str[1] = '0' + value % 10;

  return str;
}

void OnMessageReceived(char* topic, byte* payload, unsigned int length) 
{
  Serial.println(topic);
  lcd.clear();
  lcd.print(topic);

  Serial.println((char*)payload);
  lcd.setCursor(0,1);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,1);
  lcd.print((char*)payload);
  
  if (String(topic) == String(CONNECTIONS_TOPIC))
  {
    int number = rand() % 9000 + 1000;
    
    Serial.print("to connect enter");
    Serial.println(String(number));

    lcd.clear();
    lcd.print("to connect enter");
    lcd.setCursor(0,1);
    lcd.print(CLEAN_LINE);
    lcd.setCursor(0,1);
    lcd.print(String(number));

    client.publish("Device/ConnectionKeys", String(number).c_str());
  }

  if (String(topic) == String(COMMANDS_TOPIC))
  {
    Serial.print("Command: ");
    for (int i = 0; i < length; i++)
      Serial.print((char)payload[i]);

    lcd.clear();
    lcd.print("Command:");
    lcd.setCursor(0,1);
    lcd.print(CLEAN_LINE);
    lcd.setCursor(0,1);
    for (int i = 0; i < length; i++)
      lcd.print((char)payload[i]);
  
  }
}

void InitializeLCD()
{
  lcd.begin(16,2);
  Serial.println("display initialized");
  lcd.print("display initialized");
}

void InitializeTimeSync()
{
  configTime(MYTZ, "time.windows.com");

  yield();
}

void InitializeConfigServer()
{
  Serial.println("Starting server");
  lcd.setCursor(0,0);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,0);
  lcd.print("Starting server");
  
  configuration.NetworkName = String("Maloy ") + String(SERIAL_NUMBER);

  Serial.println("Connect to");
  lcd.setCursor(0,0);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,0);
  lcd.print("Connect to");

  Serial.println(configuration.NetworkName);
  lcd.setCursor(0,1);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,1);
  lcd.print(configuration.NetworkName);

  RunServer();

  while (ServerStatus() != WiFi_SUBMIT);
}

void ConnectToWifi()
{
  WiFi.mode(WIFI_STA);
  WiFi.begin(configuration.WiFiSSID, configuration.Password);

  Serial.println(configuration.WiFiSSID);
  Serial.println(configuration.Password);

  Serial.println("Connecting");
  lcd.setCursor(0,0);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,0);
  lcd.print("Connecting");

  lcd.setCursor(0,1);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,1);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
    lcd.print(".");
  }

  Serial.println("Connected");
  lcd.setCursor(0,0);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,0);
  lcd.print("Connected");

  Serial.println(WiFi.localIP());
  lcd.setCursor(0,1);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,1);
  lcd.print(WiFi.localIP());
}

void MqttReconnect() 
{
  // Loop until we're reconnected
  while (!client.connected()) 
  {
    Serial.println("Connecting to MQTT broker");
    lcd.setCursor(0,0);
    lcd.print(CLEAN_LINE);
    lcd.setCursor(0,0);
    lcd.print("Connecting to ");
    lcd.setCursor(0,1);
    lcd.print(CLEAN_LINE);
    lcd.setCursor(0,1);
    lcd.print("MQTT broker");

    String clientId = SERIAL_NUMBER;
    String password = DEVICE_PASSWORD;
    
    if (client.connect(clientId.c_str(), clientId.c_str(), password.c_str()))
    {
      Serial.println("mqtt connected");
      lcd.setCursor(0,0);
      lcd.print(CLEAN_LINE);
      lcd.setCursor(0,0);
      lcd.print("mqtt connected");
      client.subscribe(COMMANDS_TOPIC);
      client.subscribe(CONNECTIONS_TOPIC);
    }
    else
    {
      Serial.println(String("failed, rc=") + String(client.state()));
      lcd.setCursor(0,0);
      lcd.print(CLEAN_LINE);
      lcd.setCursor(0,0);
      lcd.print(String("failed, rc=") + String(client.state()));
      
      Serial.println("retry in 30 seconds");
      lcd.setCursor(0,1);
      lcd.print(CLEAN_LINE);
      lcd.setCursor(0,1);
      lcd.print("retry in 30 sec");

      delay(30000);
    }
  }
}

void InitializeMqttClient()
{
  client.setServer(configuration.BrokerAddress, configuration.Port);

  Serial.println(String(configuration.BrokerAddress) + String(":") + String(configuration.Port));

  client.setCallback(OnMessageReceived);

  Serial.println("MQTT initialized");
  lcd.setCursor(0,1);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,1);
  lcd.print("MQTT initialized");
}

void setup()
{
  Serial.begin(9600);

  InitializeLCD();

  InitializeTimeSync();

  InitializeConfigServer();

  ConnectToWifi();

  InitializeMqttClient();
}

double GetData()
{
  return millis();
}

unsigned long last;

void loop()
{
  if (!client.connected())
  MqttReconnect();

  client.loop();

  unsigned long now = millis();

  if (now - last >= 2000)
  {
    last = now;

    String curTime = ConvertTimeToString(time(nullptr));
    double value = GetData();

    String json = String("{ \"time\" : \"") + curTime + String("\", \"value\" :") + String(value) + String("}");

    client.publish("Device/Data", json.c_str());

    if (value >= 60000 && value <= 120000)
    { 
      String alarmMessage = String("Alarm! The Value is ") + String(value);

      Serial.println(alarmMessage);

      lcd.setCursor(0,0);
      lcd.print(CLEAN_LINE);
      lcd.setCursor(0,0);
      lcd.print("Alarm! The Value is ");

      lcd.setCursor(0,1);
      lcd.print(CLEAN_LINE);
      lcd.setCursor(0,1);
      lcd.print(String(value));

      client.publish(NOTIFICATION_TOPIC, alarmMessage.c_str());
    }
  }
}