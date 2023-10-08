#include <ESP8266WiFi.h>
#include <SimplePortal.h>
#include <time.h>      // time() ctime()
#include <sntp.h>  // sntp_servermode_dhcp()
#include <PubSubClient.h>
#include <TZ.h>

#define MYTZ TZ_Asia_Novosibirsk
#define SERIAL_NUMBER "SN001"
#define DEVICE_PASSWORD "AONGEhjgefjcs3299jJOJNONHFce9FvGFEBn2"
#define COMMANDS_TOPIC "Device/Commands/" SERIAL_NUMBER
#define CONNECTIONS_TOPIC "Device/Connections/" SERIAL_NUMBER
#define NOTIFICATION_TOPIC "Device/Notification/" SERIAL_NUMBER

WiFiClient espClient;
PubSubClient client(espClient);

void showTime()
{
  time_t now = time(nullptr);

  Serial.print("Converted time: ");
  Serial.println(ConvertTimeToString(now));

  // human readable
  Serial.print("time:     ");
  Serial.print(ctime(&now));

  Serial.println();
}

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
  Serial.print("Message arrived [");
  Serial.print(topic);
  Serial.print("] ");
  for (int i = 0; i < length; i++) 
    Serial.print((char)payload[i]);

  Serial.println();

  if (String(topic) == String(CONNECTIONS_TOPIC))
  {
    int number = rand() % 9000 + 1000;
    
    Serial.println(String("Connection code is ") + String(number));

    client.publish("Device/ConnectionKeys", String(number).c_str());
  }

  if (String(topic) == String(COMMANDS_TOPIC))
  {
    Serial.print("Command is");
    for (int i = 0; i < length; i++) 
    Serial.print((char)payload[i]);
    Serial.println();
  }
}

void reconnect() 
{
  // Loop until we're reconnected
  while (!client.connected()) 
  {
    Serial.print("Attempting MQTT connection...");
  
    String clientId = SERIAL_NUMBER;
    String password = DEVICE_PASSWORD;
    // Attempt to connect
    if (client.connect(clientId.c_str(), clientId.c_str(), password.c_str()))
    {
      Serial.println("connected");
      client.subscribe(COMMANDS_TOPIC);
      client.subscribe(CONNECTIONS_TOPIC);
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.println(" try again in 5 seconds");
      // Wait 30 seconds before retrying
      delay(30000);
    }
  }
}

void setup()
{
  Serial.begin(115200);
  portalCfg.SpotPointName = String("Maloy ") + String(SERIAL_NUMBER);

  portalRun(999999999999999999999999999999999999);
  
  Serial.println(portalStatus());
  // статус: 0 error, 1 connect, 2 ap, 3 local, 4 exit, 5 timeout
  
  while (portalStatus() != SP_SUBMIT);

  configTime(MYTZ, "time.windows.com");

  yield();

  WiFi.mode(WIFI_STA);
  WiFi.begin(portalCfg.SSID, portalCfg.pass);

  while (WiFi.status() != WL_CONNECTED)
  {
    delay(500);
    Serial.print(".");
  }
  Serial.println("Connected");
  Serial.println(WiFi.localIP());

  Serial.print("Broker adress: ");
  Serial.println(String(portalCfg.BrokerAdress) + ":" + String(portalCfg.Port));

  client.setServer(portalCfg.BrokerAdress, portalCfg.Port);
  client.setCallback(OnMessageReceived);
}

double GetData()
{
  return millis();
}

unsigned long last;

void loop() 
{
  if (!client.connected())
    reconnect();

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

      client.publish(NOTIFICATION_TOPIC, alarmMessage.c_str());
    }
  }
}