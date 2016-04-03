#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>

#include <WiFiClientSecure.h>
#include "WiFiSetup.h"
#include "PubSubClient.h"
#include "Logging.h"

unsigned long lastTimeCheck = 0;

#define hubAddress "arduhub.azure-devices.net"
#define hubName "pocDevice"
#define hubUser "arduhub.azure-devices.net/pocDevice"
#define hubPass "SharedAccessSignature sr=arduhub.azure-devices.net%2fdevices%2fpocDevice&sig=ksApO9qnlvs%2bERTKS3qqvO0T7cRG2D1xhI7PiE5C8uk%3d&se=1490896187"
#define hubTopic "devices/pocDevice/messages/devicebound/#"

#define Red 1
#define Green 2
#define Yellow 3

//define pins
#define RED_PIN 14
#define GREEN_PIN 12
#define YELLOW_PIN 13

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

WiFiClientSecure wclient;
PubSubClient client(wclient);

void setColor(int color) {
  if (color == Yellow) analogWrite(YELLOW_PIN, 1024/2);
  else analogWrite(YELLOW_PIN, 0);

  if (color == Green) analogWrite(GREEN_PIN, 1024/2);
  else analogWrite(GREEN_PIN, 0);

  if (color == Red) analogWrite(RED_PIN, 1024/2);
  else analogWrite(RED_PIN, 1024/2);
}

void callback(char* topic, byte* payload, unsigned int length) {
  Debug("Entered callback");
  // handle message arrived
  String msg = "";
  for (int i = 0; i < length; i++) {
    msg += (char)payload[i];
  }

  Info2("Message received: ", msg);

  if (msg.startsWith("RED")) {
    setColor(Red);
  } else if (msg.startsWith("GRN")) {
    setColor(Green);
  } else {
    setColor(Yellow);
  }
}

//TODO: do we need this method?
/*
int testWifi(void) {
  int retries = 0;
  Debug("Waiting for Wifi to connect");  
  while ( retries < 20 ) {
    if (WiFi.status() == WL_CONNECTED) { return(20); } 
    delay(500);
    Debug2("Wifi Status: " ,WiFi.status());    
    retries++;
  }
  Debug("Connect timed out, opening AP");
  return(10);
}
*/

void setup() {
  //need for debugging and communication with the slave module
  Serial.begin(74880);

  //start the eeprom and wait 10 msecs for safety
  EEPROM.begin(512);
  delay(10);

  pinMode(RED_PIN, OUTPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(YELLOW_PIN, OUTPUT);

  //initializing the wifi module
  wifiSetup.eepromOffset = 0;
  wifiSetup.loadStationSettings();

  wifiSetup.scanNetworks();

  //enter in AP mode (name should auto setup)
  Debug("Entering AP mode");
  wifiSetup.setupAP();
  delay(100);

  //wait ~20 sec for connections
  unsigned long now = millis();

  Debug("Enter setup mode for at least 20 seconds");
  wifiSetup.setupMode(20);

  Debug("Moving to station mode.");
  //TODO: if could not enter station mode (no SSID, no network, errors connecting, etc,
  //then go back to setup mode
  //time to move to Station mode
  wifiSetup.stationMode();

  Debug("Setting server for MQTT");
  //this will set the address of the hub and port on which it communicates
  client.setServer(hubAddress, 8883);

  Debug("Setting callback for MQTT");
  client.setCallback(callback);
  
  //TODO: do we need this line?
  configTime(3 * 3600, 0, "pool.ntp.org", "time.nist.gov");
}

void loop() {

  unsigned long now = millis();

  //TODO: fix bug: after 47 days the now will reset to 0
  
  //at start, or once each day
  if (lastTimeCheck == 0 || now - lastTimeCheck > 24*3600*1000) {
    lastTimeCheck = now;
    
    //TODO: check for time agains google
    //do the wifi client and shit
  }

/*
  if (WiFi.status() != WL_CONNECTED) {
    //TODO: debug info
    WiFi.begin(ssid.c_str(), pass.c_str());

    if (WiFi.waitForConnectResult() != WL_CONNECTED) {
      //TODO: log the fail reason?
      return;
    }

    //TOOD: log 
  }
*/

  Debug("Checking for wifi connected");
  if (WiFi.status() == WL_CONNECTED) {
    Debug("WiFi is connected");
    if (!client.connected()) {
      Debug("MQTT connecting..");
      if (client.connect(hubName, hubUser, hubPass)) {
        //TODO: log connected status
        Debug("MQTT connected.");

        //client.publish("outTopic", "test");
        Debug("Subscribing to the MQTT topic");
        client.subscribe(hubTopic);
        Info("Ready to receive messages.");
      } else {
        Debug("Could not connect :(");
      }
    }
  }

  Debug("checking if MQTT is connected");
  if (client.connected()) {
    Debug("MQTT is connected!");
    if (!client.loop()) {
      Debug("MQTT failed to loop");
    }
  }

  delay(1000);
}

