#include <Arduino.h>


#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <ESP8266httpUpdate.h>

#include <WiFiClientSecure.h>
#include <ESP8266HTTPClient.h>
#include "WiFiSetup.h"
#include "PubSubClient.h"
#include "Logging.h"
#include "ApiClient.h"

//don't forget to update!
char VERSION[] = "dev-10";

unsigned long lastTimeCheck = 0;

//#define hubAddress "arduhub.azure-devices.net"
//#define deviceId "pocDevice"
//#define hubUser "arduhub.azure-devices.net/pocDevice"
//#define hubPass "SharedAccessSignature sr=arduhub.azure-devices.net%2fdevices%2fpocDevice&sig=ksApO9qnlvs%2bERTKS3qqvO0T7cRG2D1xhI7PiE5C8uk%3d&se=1490896187"
//#define hubTopic "devices/pocDevice/messages/devicebound/#"

//#define SERIALSPEED 74880
//for Mini D1:
#define SERIALSPEED 57600

//define pins
#define RED_PIN 14
#define BLUE_PIN 13
#define GREEN_PIN 12

#define PATTERN_CONTINUOUS 0
#define PATTERN_WAVES 1

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

WiFiClientSecure wclient;
PubSubClient client(wclient);

bool hasColor = false;
byte pattern;
int patternStage;
byte color[3];

/*
 * Expands a 8 bit number to 10bits
 */
int expand10(byte value) {
  //ROL 2 and then ROR 6 to add the 2 upper bits as the lower ones
  int expanded = (value<<2) + (value>>6);
  return expanded;
}

void setColor(int r, int g, int b)
{
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
    
    hasColor = true;
    pattern = payload[4];
    
    patternStage = 0;
    color[0] = payload[1];
    color[1] = payload[2];
    color[2] = payload[3];

    setColor(expand10(color[0]), expand10(color[1]), expand10(color[2]));
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

void tryUpdate() {
  t_httpUpdate_return ret = ESPhttpUpdate.update("redalertxfd.azurewebsites.net", 80, "/api/iot/update", VERSION);
  switch(ret) {
      case HTTP_UPDATE_FAILED: {
            Debug("[update] Update failed.");

            int lastError = ESPhttpUpdate.getLastError();
            Debug2("Last error: ", lastError);
            String lastErrorString = ESPhttpUpdate.getLastErrorString();
            Debug2("Last error: ", lastErrorString);
        }
          break;
      case HTTP_UPDATE_NO_UPDATES:
          Debug("[update] Update no Update.");
          break;
      case HTTP_UPDATE_OK:
          Debug("[update] Update ok."); // may not called we reboot the ESP
          break;
  }
}

void setup() {
  //need for debugging and communication with the slave module
  Serial.begin(SERIALSPEED);

  //start the eeprom and wait 10 msecs for safety
  EEPROM.begin(512);
  delay(10);

  pinMode(RED_PIN, OUTPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(BLUE_PIN, OUTPUT);

  //for pin testing
  //while(true)
  {
    setColor(1023, 1023, 1023);
    delay(1000);
    /*
    setColor(0, 1023, 0);
    delay(1000);
    setColor(0, 0, 1023);
    delay(1000);
    */
    setColor(0, 0, 0);
  }

  Debug2("Version: ", VERSION);
  
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
      //cycle through the RGB colours to denote there is a problem with WiFI
      setColor(1023, 0, 0);
      setColor(0, 1023, 0);
      setColor(0, 0, 1023);
      setColor(0, 0, 0);
      //if wifi connection failed, go back to setup mode
      setupModeTimeout = 60;
      continue;
    }
  }

  //TODO: do we need this line?
  configTime(3 * 3600, 0, "pool.ntp.org", "time.nist.gov");
}

unsigned long last1000;

void loop() {
  unsigned long now = millis();
  //TODO: fix bug: after 47 days the now will reset to 0
  
  //at start, or once each day
  if (lastTimeCheck == 0 || now - lastTimeCheck > 24*3600*1000 || now < lastTimeCheck) {
    lastTimeCheck = now;

    //first we have to check for the update, as the device will restart after update
    tryUpdate();
    
    if (!apiClient.getCredentials(wifiSetup.getApiKey())) {
      //reset 
      lastTimeCheck = 0;
      
      //cycle through the RGB colours to denote there is a problem with WiFI
      setColor(1023, 0, 0);
      setColor(0, 1023, 0);
      setColor(0, 0, 1023);
      setColor(0, 0, 0);
      
      return;
    }
    
    //TODO: so what happens after 24H?...
    
    Debug("Setting server for MQTT");
    //this will set the address of the hub and port on which it communicates
    client.setServer(apiClient.getHubAddress(), 8883);
  
    Debug("Setting callback for MQTT");
    client.setCallback(callback);
  }

  //looks like making the checks so much more often, the MQTT library doesn't fail so much..
  //looks like there might be a problem if there are 2 or more MQTT messages to read fro on a MQTT loop()
  //every second
  //if (last1000 == 0 || now - last1000 >= 1000 || now < last1000) {
    last1000 = now;
    
    //Debug("Checking for wifi connected");
    if (WiFi.status() == WL_CONNECTED) {
      //Debug("WiFi is connected");
      if (!client.connected()) {
        Debug("MQTT connecting..");
        if (client.connect(apiClient.getDeviceId(), apiClient.getHubUser(), apiClient.getHubPass())) {
          //TODO: log connected status
          Debug("MQTT connected.");
  
          //client.publish("outTopic", "test");
          Debug("Subscribing to the MQTT topic and QOS1");
          client.subscribe(apiClient.getHubTopic(), 1);
          Info("Ready to receive messages.");
        } else {
          Debug("Could not connect :(");
        }
      }
      
      //Debug("checking if MQTT is connected");
      if (client.connected()) {
        //Debug("MQTT is connected!");
        if (!client.loop()) {
          Debug("MQTT failed to loop");
          Debug2("MQTT state: ", client.state());
        }
      }
    } else {
      Debug("WiFi is not connected!");

      //TODO: wifi reconnect logic in here?
    }
  //}

  //ideally will be executed every 10ms, but we don't really care about accuracy in here
  if (hasColor = true) {
    if (pattern == PATTERN_WAVES) {
      patternStage = ++patternStage % 400;
      
      //total pattern cycle is 4 sec - max stage = 400
      int red = expand10(color[0]);
      int green = expand10(color[1]);
      int blue = expand10(color[2]);
  
      //warning - we're losing precision in here, move to floating point?
      if (patternStage < 200) {
        red = (int)((float)red / 200 * patternStage);
        green = (int)((float)green / 200 * patternStage);
        blue = (int)((float)blue / 200 * patternStage);
      } else {
        red = (int)((float)red / 200 * (400 - patternStage));
        green = (int)((float)green / 200 * (400 - patternStage));
        blue = (int)((float)blue / 200 * (400 - patternStage));
      }

      setColor(red, green, blue);
    }
  }

  delay(20);
}

