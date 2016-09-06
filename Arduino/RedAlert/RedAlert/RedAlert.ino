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

extern "C" {
#include "user_interface.h"
}

//TODO: don't forget to update!
char VERSION[] = "v2-dev-12";

unsigned long lastTimeCheck = 0;

//#define hubAddress "arduhub.azure-devices.net"
//#define deviceId "pocDevice"
//#define hubUser "arduhub.azure-devices.net/pocDevice"
//#define hubPass "SharedAccessSignature sr=arduhub.azure-devices.net%2fdevices%2fpocDevice&sig=ksApO9qnlvs%2bERTKS3qqvO0T7cRG2D1xhI7PiE5C8uk%3d&se=1490896187"
//#define hubTopic "devices/pocDevice/messages/devicebound/#"

//#define SERIALSPEED 74880
//for Mini D1:
#define SERIALSPEED 57600

//switch to enable support for secondary IoT Hub
#define USE_SECONDARY_HUB false

//switch to enable light dimming based on environment light
#define USE_LIGHT_LEVEL false

//define pins
#define RED_PIN 14
#define BLUE_PIN 13
#define GREEN_PIN 12

#define PATTERN_CONTINUOUS 0
#define PATTERN_WAVES 1

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

WiFiClientSecure wclient;
WiFiClientSecure secondarywClient;
PubSubClient client(wclient);
PubSubClient secondaryClient(secondarywClient);

bool hasColor = false;
byte pattern;
int patternStage;
byte color[3];
float lastSetValue[3];
int lastLight;

/*
 * Expands a 8 bit number to 10bits
 */
int expand10(byte value) {
  //ROL 2 and then ROR 6 to add the 2 upper bits as the lower ones
  int expanded = (value<<2) + (value>>6);
  return expanded;
}

void setColor(float r, float g, float b) {
  lastSetValue[0] = r;
  lastSetValue[0] = g;
  lastSetValue[0] = b;

  if (USE_LIGHT_LEVEL) {
    float red = r;
    float green = g;
    float blue = b;

    if (lastLight > 1024 / 10 * 9) {
      //do nothing
    } else if (lastLight > 1024 / 10 * 8) {
      red = red / 10 * 9;
      green = green / 10 * 9;
      blue = blue / 10 * 9;
    } else if (lastLight > 1024 / 10 * 7) {
      red = red / 10 * 8;
      green = green / 10 * 8;
      blue = blue / 10 * 8;
    } else if (lastLight > 1024 / 10 * 6) {
      red = red / 10 * 7;
      green = green / 10 * 7;
      blue = blue / 10 * 7;
    } else if (lastLight > 1024 / 10 * 5) {
      red = red / 10 * 6;
      green = green / 10 * 6;
      blue = blue / 10 * 6;
    } else if (lastLight > 1024 / 10 * 4) {
      red = red / 10 * 5;
      green = green / 10 * 5;
      blue = blue / 10 * 5;
    } else if (lastLight > 1024 / 10 * 3) {
      red = red / 10 * 4;
      green = green / 10 * 4;
      blue = blue / 10 * 4;
    } else if (lastLight > 1024 / 10 * 2) {
      red = red / 10 * 3;
      green = green / 10 * 3;
      blue = blue / 10 * 3;
    } else if (lastLight > 1024 / 10) {
      red = red / 10 * 2;
      green = green / 10 * 2;
      blue = blue / 10 * 2;
    } else {
      red = red / 10;
      green = green / 10;
      blue = blue / 10;
    }

    analogWrite(RED_PIN, (int)red);
    analogWrite(GREEN_PIN, (int)green);
    analogWrite(BLUE_PIN, (int)blue);
  } else {
    analogWrite(RED_PIN, (int)r);
    analogWrite(GREEN_PIN, (int)g);
    analogWrite(BLUE_PIN, (int)b);
  }
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
  t_httpUpdate_return ret = ESPhttpUpdate.update("lightfeed.eu", 80, "/api/iot/update", VERSION);
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
  
  //testing the free memory TODO: monitor free memory and send as telemetry?
  /*
  uint32_t free = 0;
  {
    String s = "";
    for (int i = 0; i < 10; i++) {
      s += "test";
      free = system_get_free_heap_size();
      Debug(free);
      //s = "mama";
      delay (1000);
    }
    s = "";
    free = system_get_free_heap_size();
    Debug(free);
  }
  free = system_get_free_heap_size();
  Debug(free);
  */
  
  //start the eeprom and wait 10 msecs for safety
  EEPROM.begin(512);
  delay(10);

  pinMode(RED_PIN, OUTPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(BLUE_PIN, OUTPUT);

  //for pin testing
  //while(true)
  {
    /*
    setColor(0, 0, 0);
    //delay(0);
    int light = analogRead(A0);

    int red = 1023;
    int green = 0;
    int blue = 0;
    
    
    
    setColor(red, green, blue);
    delay(1000);
    */
    setColor(1023, 1023, 1023);
    delay(1000);
  }

  setColor(0, 0, 0);

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

//unsigned long last1000;
int ticksTo10sCounter = 0;

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
    
    if (apiClient.hasSecondary()) {
      secondaryClient.setServer(apiClient.getHubAddress2(), 8883);
      secondaryClient.setCallback(callback);
    }
  }

  //looks like making the checks so much more often, the MQTT library doesn't fail so much..
  //looks like there might be a problem if there are 2 or more MQTT messages to read fro on a MQTT loop()
  //every second
  //if (last1000 == 0 || now - last1000 >= 1000 || now < last1000) {
    //last1000 = now;
    
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
      
      if (USE_SECONDARY_HUB && apiClient.hasSecondary() && !secondaryClient.connected()) {
        Debug("Secondary MQTT connecting..");
        if (secondaryClient.connect(apiClient.getDeviceId2(), apiClient.getHubUser2(), apiClient.getHubPass2())) {
          //TODO: log connected status
          Debug("Secondary MQTT connected.");
  
          //client.publish("outTopic", "test");
          Debug("Subscribing to the Secondary MQTT topic and QOS1");
          secondaryClient.subscribe(apiClient.getHubTopic2(), 1);
          Info("Secondary Ready to receive messages.");
        } else {
          Debug("Secondary could not connect :(");
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
      
      if (USE_SECONDARY_HUB && apiClient.hasSecondary() && secondaryClient.connected()) {
        //Debug("MQTT is connected!");
        if (!secondaryClient.loop()) {
          Debug("Second MQTT failed to loop");
          Debug2("Second MQTT state: ", secondaryClient.state());
        }
      }
    } else {
      Debug("WiFi is not connected!");

      //TODO: wifi reconnect logic in here?
    }
  //}
  
  if (USE_LIGHT_LEVEL) {
    //every ~10 seconds or so
    if (ticksTo10sCounter % 500 == 0) {
      //shut down the leds to be able to do a light reading
      analogWrite(RED_PIN, 0);
      analogWrite(GREEN_PIN, 0);
      analogWrite(BLUE_PIN, 0);
      
      lastLight = analogRead(A0);
      
      //set the leds back:
      if (hasColor) {
        setColor(lastSetValue[0], lastSetValue[1], lastSetValue[2]);
      }
      
      //reset the ticks coutner
      ticksTo10sCounter = 0;
    }
    ticksTo10sCounter++;
  }

  //ideally will be executed every 20ms, but we don't really care about accuracy in here
  if (hasColor = true) {
    if (pattern == PATTERN_WAVES) {
      patternStage = ++patternStage % 400;
      
      //total pattern cycle is 4 sec - max stage = 400
      int red = expand10(color[0]);
      int green = expand10(color[1]);
      int blue = expand10(color[2]);

      float redF = 0.0;
      float greenF = 0.0;
      float blueF = 0.0;
  
      //warning - we're losing precision in here, move to floating point?
      if (patternStage < 200) {
        redF = ((float)red / 200 * patternStage);
        greenF = ((float)green / 200 * patternStage);
        blueF = ((float)blue / 200 * patternStage);
      } else {
        redF = ((float)red / 200 * (400 - patternStage));
        greenF = ((float)green / 200 * (400 - patternStage));
        blueF = ((float)blue / 200 * (400 - patternStage));
      }

      setColor(redF, greenF, blueF);
    }
  }
  
  delay(20);
}

