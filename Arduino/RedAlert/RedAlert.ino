//#include <SoftwareSerial.h>

#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>

#include <WiFiClient.h>
#include <EEPROM.h>
//#include <WiFiUdp.h>
//#include "sha256.h"
//#include "Base64.h"
#include "WiFiSetup.h"
#include "PubSubClient.h"

#define debug

#define Debug(x) Serial.print("DEBG: ");Serial.println(x)
#define Debug2(x,y) Serial.print("DEBG: ");Serial.print(x);Serial.println(y)
#define Debug3(x,y,z) Serial.print("DEBG: ");Serial.print(x);Serial.print(y);Serial.println(z)

unsigned long lastTimeCheck = 0;

//maximum length of the specific EEPROM values
#define SSID_MAX 32
#define PASS_MAX 64
#define SERIAL_MAX 36

#define hubAddress "RedAlertHubArduino.azure-devices.net"
#define hubName "what name?"
#define hubUser "hub user"
#define hubPass "hub SAS token"
#define hubTopic "some topic?"

#define Red 1
#define Green 2
#define Yellow 3

//define pins
#define RED_PIN 14
#define GREEN_PIN 12
#define YELLOW_PIN 13

//#define SOFTWARE_RX 4
//#define SOFTWARE_TX 3

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

WiFiClient wclient;
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
  // handle message arrived
  String msg = "";
  for (int i = 0; i < length; i++) {
    msg += (char)payload[i];
  }

  if (msg.startsWith("RED")) {
    setColor(Red);
  } else if (msg.startsWith("GRN")) {
    setColor(Green);
  } else {
    setColor(Yellow);
  }
}

int testWifi(void) {
  int retries = 0;
  Debug("Waiting for Wifi to connect");  
  while ( retries < 20 ) {
    if (WiFi.status() == WL_CONNECTED) { return(20); } 
    delay(500);
    Debug2("Wifi Status: " ,WiFi.status());    
    retries++;
  }
  Serial.println("Connect timed out, opening AP");
  return(10);
} 

void getDeviceId(void) {
  
}

void getSAS(void) {
  
}

void setup() {
  //need for debugging and communication with the slave module
  Serial.begin(74880);

  //start the eeprom and wait 10 msecs for safety
  EEPROM.begin(512);
  delay(10);

  pinMode(RED_PIN, OUTPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(YELLOW_PIN, OUTPUT);

  delay(5000);

  //initializing the wifi module
  wifiSetup.eepromOffset = 0;
  wifiSetup.loadStationSettings();

  //enter in AP mode (name should auto setup)
  wifiSetup.setupAP();

  //wait ~20 sec for connections
  unsigned long now = millis();

  do {
    if (wifiSetup.anyConnections()) {
      //move into setup mode
      wifiSetup.setupMode();
      
      break;
    }
  } while (millis() - now > 20000);

  //TODO: if could not enter station mode (no SSID, no network, errors connecting, etc,
  //then go back to setup mode
  //time to move to Station mode
  wifiSetup.stationMode();

  //this will set the address of the hub and port on which it communicates
  client.setServer(hubAddress, 8883);
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
  if (WiFi.status() == WL_CONNECTED) {
    if (!client.connected()) {
      if (client.connect(hubName, hubUser, hubPass)) {
        //TODO: log connected status
      }

      client.setCallback(callback);
      //client.publish("outTopic", "test");
      client.subscribe(hubTopic);
    }
  }

  if (client.connected()) {
    
    if (!client.loop()) Debug("MQTT failed to loop");
  }
}

