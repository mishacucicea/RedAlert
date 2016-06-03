#include "WiFiSetup.h"

//maximum length of the specific EEPROM values
#define SSID_MAX 32
#define PASS_MAX 64
#define SERIAL_MAX 36
//this is a magic number used to for a TRUE value for EEPROM, as EEPROM after reset can have any random value.
#define MAGIC_NUMBER B10101010

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

  //write the magic number so we know that the EEPROM values are valid
  EEPROM.write(wifiSetup.eepromOffset + SSID_MAX + PASS_MAX + SERIAL_MAX, MAGIC_NUMBER);
  
  EEPROM.commit();
  wifiSetup.loadStationSettings();
  
  String s = "<!DOCTYPE HTML>\r\n<html>Hello from ESP8266 \
  New settings saved to eeprom... reset to boot into new wifi</html>";
  
  server.send(200, "text/html", s);

  hasSetup = true;
}

void handleRoot(void) {
  String s;

        s = "\
<!doctype html>\
<html xml:lang=\"en\">\
    <head>\
        <title>Configure the LIGHT BOX</title>\
        <style>\
            body{text-align:center;font-family:Arial;color:#333333;font-size:16px;background:#f7f9f6}\
            .wrapper {background:#ffffff;padding:20px;display:inline-block;border-radius:14px;width:290px;}\
            input{border:1px solid #d2d2d2;padding:10px;}\
            input[type=\"submit\"]{background:#9fd330;color:#333333;min-width:200px;font-size:16px;padding:10px;cursor:pointer;}\
            #backButton{background:#f7f9f6;color:#333333;min-width:200px;font-size:16px;padding:10px;cursor:pointer;}\
            ol {display:inline-block;text-align:left;}\
            ol>li:nth-child(2n){background:#f7f9f6;}\
            a{text-decoration:none;color:#333333;padding:10px;display: block;}\
            a:hover{background:#9fd330;border-radius:8px;}\
        </style>\
        <script>\
            function populate(elem){\
              var contentId = document.getElementById(\"credentials\");\
              contentId.style.display == \"none\" ? contentId.style.display = \"block\" :\
              contentId.style.display = \"none\";\
              document.getElementById(\"SSID\").value = elem.getAttribute(\"ssid\");\
              document.getElementById(\"SSIDText\").innerHTML = elem.textContent + \" network:\";\
              document.getElementById(\"PASSWORD\").focus();\
              hideList();\
              showButton();\
              showCredential();\
            }\
            function showCredential(){\
              var contentId = document.getElementById(\"credentials\");\
              contentId.style.display = \"block\";\
            }\
            function showList(){\
              hideButton();\
              var contentId = document.getElementById(\"content\");\
              contentId.style.display = \"block\";\
              hideCredential();\
            }\
            function hideList(){\
              var contentId = document.getElementById(\"content\");\
              contentId.style.display = \"none\";\
            }\
            function hideButton() {\
              var contentId = document.getElementById(\"buttonID\");\
              contentId.style.display = \"none\";\
            }\
            function showButton() {\
              var contentId = document.getElementById(\"buttonBackID\");\
              contentId.style.display = \"block\";\
            }\
            function hideCredential() {\
              var contentId = document.getElementById(\"credentials\");\
              contentId.style.display = \"none\";\
            }\
        </script>\
    </head>\
    <body>\
        <div class=\"wrapper\">\
            <p style=\"display:none;\" id=\"buttonID\">  <input type=\"button\" value=\"Select network\" onclick=\"showList()\"/></p>\
           <div ID=\"content\" style=\"display:block;\">\
             <p><b>Please select your wireless network:</b></p>";
        s += st;
        s += "\
            </div>\
            <form method='get' action='setup' id=\"credentials\"  style=\"display:none;\">\
              <input id=\"SSID\" type=\"hidden\" name=\"ssid\" />\
                <div id=\"SSIDText\"></div></br>\
                <div >\
                  <input id=\"PASSWORD\" type=\"password\" placeholder=\"wireless password\" name=\"pass\"/>\
                </div></br>\
                <div>\
                  <input type=\"text\" placeholder=\"device ID\" name=\"serial\"/>\
                </div></br>\
                  <input type=\"submit\" value=\"Submit\" />\
                    <p style=\"display:none;\" id=\"buttonBackID\">\
                  <input type=\"button\" id=\"backButton\" value=\"Back\" onclick=\"showList()\"/></p>\
            </form>\
        </div>\
    </body>\
</html>";
  server.send(200, "text/html", s);
}

void handleNotFound(void) {
  server.send(404, "text/html", "Nope dude..");
}

WiFiSetup::WiFiSetup(void) {
  eepromOffset = 0;
  hasSettings = false;
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

  st = "";
  for (int i = 0; i < n; ++i)
    {
      //new template:
      //<a href="#" onclick="populate(this)">Dinamitescu <i> (strong)</i> </a>
      //TODO: add signal strength
      st += "<a href=\"#\" onclick=\"populate(this)\" ssid=\"" + WiFi.SSID(i) + "\">" + WiFi.SSID(i) + "</a>";
    }
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

  hasSettings = false;

  Debug("Checking if settings are stored in EEPROM");
  hasSettings = MAGIC_NUMBER == EEPROM.read(eepromOffset + SSID_MAX + PASS_MAX + SERIAL_MAX);

  if (hasSettings) {
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
  } else {
    Debug("No settings stored in EEPROM");
  }
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

boolean WiFiSetup::getHasSettings(void) {
  return hasSettings;
}

void WiFiSetup::beginSetupMode(int seconds) {
  hasSetup = false;
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
}

void WiFiSetup::endSetupMode(void) {
  Debug("Stopping and closing the server");
  server.stop();
  //panic mode if close()..
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
