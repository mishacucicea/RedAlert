
#ifndef WiFiSetup_h
#define WiFiSetup_H


#include <ESP8266mDNS.h>
#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>
#include <EEPROM.h>
#include "PubSubClient.h"
#include "Arduino.h"
#include "Logging.h"

class WiFiSetup {
  public:
    WiFiSetup(void);
    void scanNetworks(void);
    void setupAP(void);
    bool anyConnections(void);
    /*
     * Loads the SSID, pass and serial number into wifi
     */
    void loadStationSettings(void);
    String getSsid(void);
    String getPass(void);
    String getSerial(void);
    boolean getHasSettings(void);
    
    /*
     * Launches the web server with the setup page.
     */
    void beginSetupMode(int seconds);
    void endSetupMode(void);
    bool stationMode(void);

    int eepromOffset;
  private:
    //the html for the list of available stations
    String ssid;
    String pass;
    String serial;
    bool hasSettings;
};

extern WiFiSetup wifiSetup;

#endif
//t_httpUpdate_return ret = ESPhttpUpdate.update("192.168.0.2", 80, "/esp/update/arduino.php", "optional current version string here");

