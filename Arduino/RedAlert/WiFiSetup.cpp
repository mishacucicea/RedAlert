#include "WiFiSetup.h"

//maximum length of the specific EEPROM values
#define SSID_MAX 32
#define PASS_MAX 64
#define SERIAL_MAX 36

#define Debug(x) Serial.print("DEBG: ");Serial.println(x)
#define Debug2(x,y) Serial.print("DEBG: ");Serial.print(x);Serial.println(y)
#define Debug3(x,y,z) Serial.print("DEBG: ");Serial.print(x);Serial.print(y);Serial.println(z)

char* deviceSSID = "RedAlert-123";
String st;
bool hasSetup = false; 

ESP8266WebServer server(80);

void handleSetup(void) {
  Debug("clearing eeprom");
  for (int i = 0; i < SSID_MAX + PASS_MAX + SERIAL_MAX; ++i) { EEPROM.write(i, 0); }
  
  String qsid = server.arg("ssid");
  Debug2("New SSID: ", qsid);
  
  String qpass = server.arg("pass");
  Debug2("New Password: ", qpass);
  
  String qsn = server.arg("serial");
  Debug2("New Serial Number: ", qsn);
  
  Debug("writing eeprom ssid");
  for (int i = 0; i < qsid.length(); ++i) {
    EEPROM.write(wifiSetup.eepromOffset + i, qsid[i]);
  }
  
  Debug("writing eeprom pass"); 
  for (int i = 0; i < qpass.length(); ++i) {
    EEPROM.write(wifiSetup.eepromOffset + SSID_MAX + i, qpass[i]);
  }    
  
  Debug("wrigin eeprom serial number");
  for (int i = 0; i < qsn.length(); ++i) {
    EEPROM.write(wifiSetup.eepromOffset + SSID_MAX + PASS_MAX + i, qsn[i]);
  }
  
  EEPROM.commit();
  
  String s = "<!DOCTYPE HTML>\r\n<html>Hello from ESP8266 \
  New settings saved to eeprom... reset to boot into new wifi</html>";
  
  server.send(200, "text/html", s);

  hasSetup = true;
}

void handleRoot(void) {
  String s;

        s = "<!DOCTYPE HTML>\r\n\
<html>Hello from RedAlert";
        s += "<p>";
        s += st;
        s += "\
  <form method='get' action='setup'>\
    <label>SSID: </label>\
    <input name='ssid' length=32>\
    <input name='pass' length=64>\
    <input name='serial' length=36>\
    <input type='submit'>\
  </form>\
</html>";
  server.send(200, "text/html", s);
}

void handleNotFound(void) {
  server.send(404, "text/html", "Nope dude..");
}

WiFiSetup::WiFiSetup(void) {
  eepromOffset = 0;
}

void WiFiSetup::scanNetworks(void) {
  Debug("Switching STA mode (Station)");
  WiFi.mode(WIFI_STA);
  
  //Debug("Disconnecting");
  //WiFi.disconnect();
  delay(100);
  
  Debug("Scanning networks");
  int n = WiFi.scanNetworks();
  Debug("scan done");
  
  if (n == 0) {
    Debug("no networks found");
  }
  else {
    Debug("Networks found:");
    for (int i = 0; i < n; ++i)
     {
      Debug(String(WiFi.SSID(i)) + " (" + WiFi.RSSI(i) + ")" + 
          ((WiFi.encryptionType(i) == ENC_TYPE_NONE)?" ":"*"));
      delay(10);
     }
  }

  st = "<ul>";
  for (int i = 0; i < n; ++i)
    {
      // Print SSID and RSSI for each network found
      st += "<li>";
      st +=i + 1;
      st += ": ";
      st += WiFi.SSID(i);
      st += " (";
      st += WiFi.RSSI(i);
      st += ")";  
      st += (WiFi.encryptionType(i) == ENC_TYPE_NONE)?" ":"*";
      st += "</li>";
    }
  st += "</ul>";
}

void WiFiSetup::setupAP(void) {
  WiFi.mode(WIFI_AP);
  IPAddress apIP(192, 168, 1, 1);
  WiFi.softAPConfig(apIP, apIP, IPAddress(255, 255, 255, 0));
  WiFi.softAP(deviceSSID);
}

bool WiFiSetup::anyConnections(void)  {
  if (WiFi.softAPgetStationNum() > 0) {
    Debug("Station connected");
    return true;
  }
  else return false;
}

/*
 * Loads the SSID, pass and serial number into wifi
 */
void WiFiSetup::loadStationSettings(void) {
  ssid = "";
  pass = "";
  serial = "";
  
  Debug("Reading EEPROM ssid");
  for (int i = 0; i < SSID_MAX; ++i) {
      ssid += char(EEPROM.read(eepromOffset + i));
  }
  Debug2("SSID: ", ssid);
  
  Debug("Reading EEPROM pass");
  for (int i = SSID_MAX; i < SSID_MAX + PASS_MAX; ++i) {
      pass += char(EEPROM.read(eepromOffset + i));
  }
  Debug2("Password: ", pass);

  Debug("Reading EEPROM Serial Number");
  for (int i = SSID_MAX + PASS_MAX; i < SSID_MAX + PASS_MAX + SERIAL_MAX; ++i) {
    serial += char(EEPROM.read(eepromOffset + i));
  }
  Debug2("Hub: ", serial);
}

String WiFiSetup::getSsid(void) {
  return ssid;
}
String WiFiSetup::getPass(void) {
  return pass;
}
String WiFiSetup::getSerial(void) {
  return serial;
}

void WiFiSetup::setupMode(int seconds) {
  Debug("Entered setupMode");

  Debug2("Assigned IP: ", WiFi.softAPIP());
  if (!MDNS.begin("redalert", WiFi.softAPIP())) {
    Debug("Error setting up MDNS responder!");

    //TODO: figure out what to do, otherwise it will just hang..
    //TODO: set some visual code
    while(1) { 
      delay(1000);
    }
  }
  Debug("mDNS responder started");

  server.on("/", HTTP_GET, handleRoot);
  server.on("/setup", HTTP_GET, handleSetup);

  server.onNotFound(handleNotFound);

  server.begin();
  
  // Start the server
  server.begin();
  Debug("Server started");   

  unsigned long now = millis();
  
  while (!hasSetup && (seconds == 0 || (millis() - now < (seconds * 1000)) )) {
    server.handleClient();
    if (server.client()) {
      Debug("Client connected.");
      seconds = 0;
    }
  }

  Debug("Stopping and closing the server");
  server.stop();
  //server.close();
}

bool WiFiSetup::stationMode(void) {
  int retries = 0;
  Debug("Connecting to the AP as Station");

  Debug("setting mode to STA");
  WiFi.mode(WIFI_STA);
  Debug("Disconnecting from previous wifi");
  WiFi.disconnect();
  Debug("Trying to connect..");
  WiFi.begin(ssid.c_str(), pass.c_str());
  Debug("Checking for connected status..");
  while ( retries < 20 ) {
    if (WiFi.status() == WL_CONNECTED) { 
      
      Debug("Connected to wifi! Local IP:");
      Debug(WiFi.localIP());
      return(true); 
    } 
    delay(500);
    Debug2("Wifi Status: " ,WiFi.status());    
    retries++;
  }

  Serial.println("Connect timed out");
  return(false);
}

WiFiSetup wifiSetup = WiFiSetup();
