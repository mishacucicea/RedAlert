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

//define pins
#define RED_PIN 14
#define BLUE_PIN 13
#define GREEN_PIN 12

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

WiFiClientSecure wclient;
PubSubClient client(wclient);

void setColor(int r, int g, int b)
{
  //not exact, there is some loss in values.
  analogWrite(RED_PIN, r);
  analogWrite(GREEN_PIN, g);
  analogWrite(BLUE_PIN, b);
}

void callback(char* topic, byte* payload, unsigned int length) {
  Debug("Entered callback");
  //this is the new format:
  //check that the length is 9 and message code is 1
  if (length == 9 && payload[0] == 1) {
    //1 byte is the type of the message
    //3 bytes is the RGB value
    //1 bytes is the pattern number - not supported yet
    //4 bytes is the timeout - not supported yet
    setColor(payload[1], payload[2], payload[3]);
  }
  else {

    // handle message arrived
    String msg = "";
    for (int i = 0; i < length; i++) {
      msg += (char)payload[i];
    }
  
    Info2("Message received: ", msg);
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
  pinMode(BLUE_PIN, OUTPUT);

  //for pin testing
  //while(true)
  {
    setColor(255, 0, 0);
    delay(1000);
    setColor(0, 255, 0);
    delay(1000);
    setColor(0, 0, 255);
    delay(1000);
    setColor(0, 0, 0);
  }
  
  //initializing the wifi module
  wifiSetup.eepromOffset = 0;
  wifiSetup.loadStationSettings();

  int setupModeTimeout = 40;

  while (true) {
    wifiSetup.scanNetworks();
  
    //enter in AP mode (name should auto setup)
    Debug("Entering AP mode");
    wifiSetup.setupAP();
    delay(100);
  
    //wait ~40 sec for connections
    unsigned long now = millis();
  
    Debug("Enter setup mode for at least 40 seconds");
    do  {
      wifiSetup.beginSetupMode(setupModeTimeout);
    } while (!wifiSetup.getHasSettings());
  
    wifiSetup.endSetupMode();
    
    Debug("Moving to station mode.");
    //TODO: if could not enter station mode (no SSID, no network, errors connecting, etc,
    //then go back to setup mode
    //time to move to Station mode
    if (wifiSetup.stationMode()) {
      break;
    }
    else {
      //if wifi connection failed, go back to setup mode
      setupModeTimeout = 60;
      continue;
    }
  }

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
  if (lastTimeCheck == 0 || now - lastTimeCheck > 24*3600*1000 || now < lastTimeCheck) {
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
    
    Debug("checking if MQTT is connected");
    if (client.connected()) {
      Debug("MQTT is connected!");
      if (!client.loop()) {
        Debug("MQTT failed to loop");
      }
    }
  }

  delay(1000);
}

