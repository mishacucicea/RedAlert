//#include <SoftwareSerial.h>

#include <ESP8266WiFi.h>
//#include <WiFiUdp.h>
#include "sha256.h"
#include "Base64.h"
#include "PubSubClient.h"

char* ssid = "WIFI_SID";
char* pass = "WIFI_PASS";

unsigned long lastTimeCheck = 0;

#define hubName "what name?"
#define hubUser "hub user"
#define hubPass "hub SAS token"
#define hubTopic "some topic?"

//#define SOFTWARE_RX 4
//#define SOFTWARE_TX 3

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

IPAddress server(172, 16, 0, 2); //TODO: set IoT Hub IP
WiFiClient wclient;
PubSubClient client(wclient, server);

void callback(const MQTT::Publish& pub) {
  // handle message arrived
}

void setup() {
  // put your setup code here, to run once:

  //TODO: need to setup the software serial, review the actual pins
  //SoftwareSerial Serial1(SOFTWARE_RX, SOFTWARE_TX);


  //this piece of code should try to connect to the wifi 3 times
//  int connectRetries = 0;
//  bool connected = false;
//  while (!(connected = connectWiFi) && connectRetries++ < 3) { }

  //datep = get_date_from_header(datep);
}

void loop() {

  if (WiFi.status() != WL_CONNECTED) {
    //TODO: debug info
    WiFi.begin(ssid, pass);

    if (WiFi.waitForConnectResult() != WL_CONNECTED) {
      //TODO: log the fail reason?
      return;
    }

    //TOOD: log 
  }

  if (WiFi.status() == WL_CONNECTED) {
    if (!client.connected()) {
      if (client.connect(MQTT::Connect(hubName).set_auth(hubUser, hubPass))) {
        //TODO: log connected status
      }

      client.set_callback(callback);
      //client.publish("outTopic", "test");
      client.subscribe(hubTopic);
    }
  }

  unsigned long now = millis();
  //at start, or once each day
  if (lastTimeCheck == 0 || now - lastTimeCheck > 24*3600*1000) {
    lastTimeCheck = now;
    
    //TODO: check for time agains google
    //do the wifi client and shit
  }

  if (client.connected()) {
    //TODO: log if false
    client.loop();
  }
}




/*
  ////testing base64
  // put your main code here, to run repeatedly:
  char input[] = "Hello world";
  int inputLen = sizeof(input);
  
  int encodedLen = base64_enc_len(inputLen);
  char encoded[encodedLen];
  base64_encode(encoded, input, inputLen);

  ////testing HMAC-SHA-256
  uint8_t *hash;
  Sha256.initHmac("hash key",8); // key, and length of key in bytes
  Sha256.print("This is a message to hash");
  hash = Sha256.resultHmac();
  */


