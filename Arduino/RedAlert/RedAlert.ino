#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <ESP8266httpUpdate.h>

#include <WiFiClientSecure.h>
#include <ESP8266HTTPClient.h>
#include "WiFiSetup.h"
#include "PubSubClient.h"
#include "Logging.h"

//don't forget to update!
char VERSION[] = "dev-05";

unsigned long lastTimeCheck = 0;

//#define hubAddress "arduhub.azure-devices.net"
//#define deviceId "pocDevice"
//#define hubUser "arduhub.azure-devices.net/pocDevice"
//#define hubPass "SharedAccessSignature sr=arduhub.azure-devices.net%2fdevices%2fpocDevice&sig=ksApO9qnlvs%2bERTKS3qqvO0T7cRG2D1xhI7PiE5C8uk%3d&se=1490896187"
//#define hubTopic "devices/pocDevice/messages/devicebound/#"

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

char hubAddress[100];
char deviceId[100];
char hubUser[100];
char hubPass[256];
char hubTopic[100];

bool hasColor = false;
byte pattern;
int patternStage;
byte color[3];

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
    setColor(payload[1]<<2, payload[2]<<2, payload[3]<<2);

    hasColor = true;
    pattern = payload[4];
    patternStage = 0;
    color[0] = payload[1];
    color[1] = payload[2];
    color[2] = payload[3];
    
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
    setColor(1023, 0, 0);
    delay(1000);
    setColor(0, 1023, 0);
    delay(1000);
    setColor(0, 0, 1023);
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
    t_httpUpdate_return ret = ESPhttpUpdate.update("redalertxfd.azurewebsites.net", 80, "/api/iot/update", VERSION);
    switch(ret) {
        case HTTP_UPDATE_FAILED:
            Debug("[update] Update failed.");
            break;
        case HTTP_UPDATE_NO_UPDATES:
            Debug("[update] Update no Update.");
            break;
        case HTTP_UPDATE_OK:
            Debug("[update] Update ok."); // may not called we reboot the ESP
            break;
    }
    
    //TODO: check for time agains google
    //do the wifi client and shit
    String s = "http://redalertxfd.azurewebsites.net/api/iot/authentication?devicekey=";
    s += wifiSetup.getApiKey();
    
    Debug2("Making HTTP request to:", s);
    
    HTTPClient http;
    http.begin(s);
    http.setTimeout(30000);
    int httpCode = http.GET();

    if (httpCode > 0) {
      if (httpCode == HTTP_CODE_OK) {
        Debug("Getting authetnication data");
        String payload = http.getString();
        //now we have to split the payload
        int index = payload.indexOf('\n');
        String hubAddress_ = payload.substring(0, index);
        hubAddress_.trim();
        hubAddress_.toCharArray(hubAddress, hubAddress_.length() + 1);
        payload.remove(0, index+1);

        index = payload.indexOf('\n');
        String deviceId_ = payload.substring(0, index);
        deviceId_.trim();
        deviceId_.toCharArray(deviceId, deviceId_.length() + 1);
        payload.remove(0, index+1);

        index = payload.indexOf('\n');
        String hubUser_ = payload.substring(0, index);
        hubUser_.trim();
        hubUser_.toCharArray(hubUser, hubUser_.length() + 1);
        payload.remove(0, index+1);

        index = payload.indexOf('\n');
        String hubPass_ = payload.substring(0, index);
        hubPass_.trim();
        hubPass_.toCharArray(hubPass, hubPass_.length() + 1);
        payload.remove(0, index+1);

        //what's left is the hubTopic
        payload.trim();
        payload.toCharArray(hubTopic, payload.length() + 1);

        //just for testing:
        Debug2("hubAddress: ", hubAddress);
        Debug2("deviceId: ", deviceId);
        Debug2("hubUser: ", hubUser);
        Debug2("hubPass: ", hubPass);
        Debug2("hubTopic: ", hubTopic);
      }
      else {
        //TODO: what do we do?
        Debug2("Error status code", httpCode); 
      }
    }
    else {
      //TODO: an error occurred, what do we do?
      Debug2("Failed to GET: ", httpCode);
    }

    //TODO: so what happens after 24H?...
    
    Debug("Setting server for MQTT");
    //this will set the address of the hub and port on which it communicates
    client.setServer(hubAddress, 8883);
  
    Debug("Setting callback for MQTT");
    client.setCallback(callback);
  }

  //every second
  if (last1000 == 0 || now - last1000 >= 1000 || now < last1000) {
    last1000 = now;
    
    Debug("Checking for wifi connected");
    if (WiFi.status() == WL_CONNECTED) {
      Debug("WiFi is connected");
      if (!client.connected()) {
        Debug("MQTT connecting..");
        if (client.connect(deviceId, hubUser, hubPass)) {
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
  }

  //ideally will be executed every 10ms, but we don't really care about accuracy in here
  if (hasColor = true) {
    if (pattern == PATTERN_WAVES) {
      patternStage = ++patternStage % 400;
      
      //total pattern cycle is 4 sec - max stage = 400
      int red = color[0] << 2;
      int green = color[1] << 2;
      int blue = color[2] << 2;
  
      //warning - we're losing precision in here, move to floating point?
      if (patternStage < 200) {
        red = red / 200 * patternStage;
        green = green / 200 * patternStage;
        blue = blue / 200 * patternStage;
      } else {
        red = red / 200 * (400 - patternStage);
        green = green / 200 * (400 - patternStage);
        blue = blue / 200 * (400 - patternStage);
      }
      setColor(red, green, blue);
    }
  }

  delay(10);
}

