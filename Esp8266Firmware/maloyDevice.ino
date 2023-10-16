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

void LcdPrint(int line, const char* message)
{
  lcd.setCursor(0, line);
  lcd.print(CLEAN_LINE);
  lcd.setCursor(0,line);
  lcd.print(message);
}

void LcdPrint(int line, String message)
{
  LcdPrint(line, message.c_str());
}

void OnMessageReceived(char* topic, byte* payload, unsigned int length) 
{
  Serial.println(topic);
  LcdPrint(0, topic);

  char* message = new char[length + 1];

  for (int i = 0; i < length; i ++)
    message[i] = (char)payload[i];

  message[length] = 0;

  Serial.println(message);
  
  if (String(topic) == String(CONNECTIONS_TOPIC))
  {
    if (String(message) == String("connect"))
    {
      srand(time(0));
      int number = rand() % 9000 + 1000;
    
      Serial.print("to connect enter ");
      Serial.println(String(number));

      LcdPrint(0, "to connect enter");
      LcdPrint(1, String(number));

      client.publish("Device/ConnectionKeys", String(number).c_str());
    }
    else if (String(message) == String("cancel"))
    {
      lcd.clear();

      client.publish("Device/ConnectionKeys", String(-1).c_str());
    }
  }

  if (String(topic) == String(COMMANDS_TOPIC))
  {   
    int commandLength = strcspn(message, "/") + 1;

    char* command = new char[commandLength];
    char* content = message + commandLength;

    for (int i = 0; i < commandLength; i++)
      command[i] = message[i];

    command[commandLength - 1] = 0;

    if (String(command) == String("print"))
    {
      Serial.print(content);
      LcdPrint(0, content);

      client.publish(NOTIFICATION_TOPIC, (String("invoked command: ") + String(command)).c_str());
    }
    else if (String(command) == String("restart"))
    {
      client.publish(NOTIFICATION_TOPIC, (String("invoked command: ") + String(command)).c_str());

      ESP.reset();
    }
    else
    {
      client.publish(NOTIFICATION_TOPIC, (String("invoked command: ") + String(message)).c_str());

      LcdPrint(0, message);
    }

    delete[] command;
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
  LcdPrint(0, "Starting server");
  
  configuration.NetworkName = String("Maloy ") + String(SERIAL_NUMBER);

  Serial.println("Connect to");
  LcdPrint(0, "Connect to");

  Serial.println(configuration.NetworkName);
  LcdPrint(1, configuration.NetworkName);

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
  LcdPrint(0, "Connecting");

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
  LcdPrint(0, "Connected");
}

void MqttReconnect() 
{
  // Loop until we're reconnected
  while (!client.connected()) 
  {
    Serial.println("Connecting to MQTT broker");
    LcdPrint(0, "Connecting to");
    LcdPrint(1, "MQTT broker");

    String clientId = SERIAL_NUMBER;
    String password = DEVICE_PASSWORD;
    
    if (client.connect(clientId.c_str(), clientId.c_str(), password.c_str()))
    {
      Serial.println("mqtt connected");
      LcdPrint(0, "mqtt connected");
      client.subscribe(COMMANDS_TOPIC);
      client.subscribe(CONNECTIONS_TOPIC);
      client.publish(NOTIFICATION_TOPIC, "connected to server");
    }
    else
    {
      Serial.println(String("failed, rc=") + String(client.state()));
      LcdPrint(0, String("failed, rc=") + String(client.state()));
      
      Serial.println("retry in 30 seconds");
      LcdPrint(1, "retry in 5 sec");

      delay(5000);
    }
  }
}

void InitializeMqttClient()
{
  client.setServer(configuration.BrokerAddress, configuration.Port);

  client.setCallback(OnMessageReceived);

  Serial.println("MQTT initialized");
  LcdPrint(1, "MQTT initialized");
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

    unsigned long int unixTime = time(nullptr);
    double value = GetData();

    String payload = String(unixTime) + String("_") + String(value);

    client.publish("Device/Data", payload.c_str());

    if (value >= 60000 && value <= 120000)
    { 
      String alarmMessage = String("Alarm! The Value is ") + String(value);

      Serial.println(alarmMessage);

      LcdPrint(0, "Alarm! Value is ");
      
      LcdPrint(1, String(value));
      
      client.publish(NOTIFICATION_TOPIC, alarmMessage.c_str());
    }
  }
}