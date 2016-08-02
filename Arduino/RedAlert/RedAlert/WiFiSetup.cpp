#include "WiFiSetup.h"

//maximum length of the specific EEPROM values
#define SSID_MAX 32
#define PASS_MAX 64
#define APIKEY_MAX 36
//this is a magic number used to for a TRUE value for EEPROM, as EEPROM after reset can have any random value.
#define MAGIC_NUMBER B10101010

String st;
bool hasSetup = false;

ESP8266WebServer server(80);

void handleSetup(void) {
  Debug("clearing eeprom");
  for (int i = 0; i < SSID_MAX + PASS_MAX + APIKEY_MAX; ++i) { EEPROM.write(i, 0); }

  String qsid = server.arg("ssid");
  qsid.trim();
  Debug2("New SSID: ", qsid);

  String qpass = server.arg("pass");
  qpass.trim();
  Debug2("New Password: ", qpass);


  String qsn = server.arg("apikey");
  qsn.trim();
  Debug2("New API Key: ", qsn);

  Debug("writing eeprom ssid");
  for (int i = 0; i < qsid.length(); ++i) {
    EEPROM.write(wifiSetup.eepromOffset + i, qsid[i]);
  }

  Debug("writing eeprom pass");
  for (int i = 0; i < qpass.length(); ++i) {
    EEPROM.write(wifiSetup.eepromOffset + SSID_MAX + i, qpass[i]);
  }

  Debug("wrigin eeprom API Key");
  for (int i = 0; i < qsn.length(); ++i) {
    EEPROM.write(wifiSetup.eepromOffset + SSID_MAX + PASS_MAX + i, qsn[i]);
  }

  //write the magic number so we know that the EEPROM values are valid
  EEPROM.write(wifiSetup.eepromOffset + SSID_MAX + PASS_MAX + APIKEY_MAX, MAGIC_NUMBER);

  EEPROM.commit();
  wifiSetup.loadStationSettings();
  
  server.send(200, "text/html", "{}");

  hasSetup = true;
}

void handleRoot(void) {
  String s;
  //TODO: in next version of ESP8266 Aarduino core (probably 2.4.0) chunks will be supported, and should
  //alow us to store less strings in the memory as we'll send the body in chunks
  s = "\
  <!doctype html>\n\
  <html xml:lang=\"en\">\n\
    <head>\n\
      <meta name=\"viewport\" content=\"width=device-width, initial-scale=1\">\n\
        <title>Configure the LIGHT BOX</title>\n\
        <style>\n\
          body{text-align:center;font-family:Arial;color:#333333;font-size:16px;background:#f7f9f6}\n\
          .wrapper {background:#ffffff;padding:20px;display:inline-block;border-radius:14px;width:290px;}\n\
          input{border:1px solid #d2d2d2;padding:10px;}\n\
          input.submit{background:#9fd330;color:#333333;min-width:200px;font-size:16px;padding:10px;cursor:pointer;}\n\
          #backButton{background:#f7f9f6;color:#333333;min-width:200px;font-size:16px;padding:10px;cursor:pointer;}\n\
          ol {display:inline-block;text-align:left;}\n\
          ol>li:nth-child(2n){background:#f7f9f6;}\n\
          a{text-decoration:none;color:#333333;padding:10px;display: block;}\n\
          a:hover{background:#9fd330;border-radius:8px;}\n\
        </style>\n\
        <script>\n\
          function populate(elem){\n\
            var contentId = document.getElementById(\"credentials\");\n\
            contentId.style.display == \"none\" ? contentId.style.display = \"block\" :\n\
            contentId.style.display = \"none\";\n\
            document.getElementById(\"SSID\").value = elem.getAttribute(\"ssid\");\n\
            document.getElementById(\"SSIDText\").innerHTML = elem.textContent + \" network:\";\n\
            document.getElementById(\"PASSWORD\").focus();\n\
            hideList();\n\
            showButton();\n\
            showCredential();\n\
          }\n\
          function showCredential(){\n\
            var contentId = document.getElementById(\"credentials\");\n\
            contentId.style.display = \"block\";\n\
          }\n\
          function showList(){\n\
            hideButton();\n\
            var contentId = document.getElementById(\"content\");\n\
            contentId.style.display = \"block\";\n\
            hideCredential();\n\
          }\n\
          function hideList(){\n\
            var contentId = document.getElementById(\"content\");\n\
            contentId.style.display = \"none\";\n\
          }\n\
          function hideButton() {\n\
            var contentId = document.getElementById(\"buttonID\");\n\
            contentId.style.display = \"none\";\n\
          }\n\
          function showButton() {\n\
            var contentId = document.getElementById(\"buttonBackID\");\n\
            contentId.style.display = \"block\";\n\
          }\n\
          function hideCredential() {\n\
            var contentId = document.getElementById(\"credentials\");\n\
            contentId.style.display = \"none\";\n\
          }\n\
          function checkWiFi() {\n\
            /*TODO: show something*/\n\
            var checkwifiId = document.getElementById(\"checkwifi\");\n\
            checkwifiId.style.display=\"block\";\n\
            var contentId = document.getElementById(\"credentials\");\n\
            contentId.style.display = \"none\";\n\
            var xmlhttp = new XMLHttpRequest();\n\
            var ssid = document.getElementById(\"SSID\").value\n\
            var ssidPass = document.getElementById(\"PASSWORD\").value\n\
            var apikey = document.getElementById(\"APIKEY\").value\n\
            xmlhttp.onreadystatechange = function() {\n\
              if (xmlhttp.readyState == 4) {\n\
                if (xmlhttp.status == 200) {\n\
                  var r = JSON.parse(xmlhttp.responseText);\n\
                  if (r.retry == true) {\n\
                    setTimeout(function(){\n\
                      xmlhttp.open(\"GET\", \"checkwifi?ssid=\"+ssid+\"&pass=\"+ssidPass, true);\n\
                      xmlhttp.send();\n\
                    }, 500);\n\
                  } else if (r.connected) {\n\
                    var checkwifiId = document.getElementById(\"checkwifi\");\n\
                    checkwifiId.style.display=\"none\";\n\
                    var checkapikeyId = document.getElementById(\"checkapikey\");\n\
                    checkapikeyId.style.display = \"block\";\n\
                    xmlHttpHub = new XMLHttpRequest();\n\
                    xmlHttpHub.onreadystatechange = function() {\n\
                      if (xmlHttpHub.readyState == 4) {\n\
                        if (xmlHttpHub.status == 200) {\n\
                          r = JSON.parse(xmlHttpHub.responseText);\n\
                          if (r.valid) {\n\
                            var checkapikeyId = document.getElementById(\"checkapikey\");\n\
                            checkapikeyId.style.display = \"none\";\n\
                            var doneId = document.getElementById(\"done\");\n\
                            doneId.style.display = \"block\";\n\
                            xmlSetup = new XMLHttpRequest();\n\
                            xmlSetup.open(\"GET\", \"setup?ssid=\"+ssid+\"&pass=\"+ssidPass+\"&apikey=\"+apikey, true);\n\
                            xmlSetup.send();\n\
                          } else {\n\
                            var contentId = document.getElementById(\"checkapikeyproblem\");\n\
                            contentId.style.display = \"block\";\n\
                          }\n\
                        }\n\
                      }\n\
                    }\n\
                    xmlHttpHub.open(\"GET\", \"checkapi?apikey=\"+apikey, true);\n\
                    xmlHttpHub.send();\n\
                  } else {\n\
                    var contentId = document.getElementById(\"checkwifiproblem\");\n\
                    contentId.style.display = \"block\";\n\
                  }\n\
                } else {\n\
                  var contentId = document.getElementById(\"checkwifiproblem\");\n\
                  contentId.style.display = \"block\";\n\
                  /*TODO: something went wrong, is it even possible?*/\n\
                }\n\
              }\n\
            };\n\
            xmlhttp.open(\"GET\", \"checkwifi?ssid=\"+ssid+\"&pass=\"+ssidPass, true);\n\
            xmlhttp.send();\n\
          }\n\
        </script>\n\
      </head>\n\
      <body>\n\
        <div class=\"wrapper\">\n\
          <p style=\"display:none;\" id=\"buttonID\">  <input type=\"button\" value=\"Select network\" onclick=\"showList()\"/></p>\n\
          <div ID=\"content\" style=\"display:block;\">\n\
            <p><b>Please select your wireless network:</b></p>\n";
      s += st;
      s += "\
          </div>\n\
          <div id=\"checkwifi\" style=\"display:none;\">\n\
            <p><b>Checking WiFi..</b></p>\n\
            <div id=\"checkwifiproblem\" style=\"display:none\">\n\
              <p>There was a problem connecting to the WiFi. Please try again.</p>\n\
              <form method='get' action='/'>\n\
                <input type=\"submit\" class=\"submit\" value=\"Reset\" />\n\
              </form>\n\
            </div>\n\
          </div>\n\
          <div id=\"checkapikey\" style=\"display:none;\">\n\
            <p><b>Checking API Key..</b></p>\n\
            <div id=\"checkapikeyproblem\" style=\"display:none\">\n\
              <p>There was a problem checking the API Key. Please try again.</p>\n\
              <form method='get' action='/'>\n\
                <input type=\"submit\" class=\"submit\" value=\"Reset\" />\n\
              </form>\n\
            </div>\n\
          </div>\n\
          <div id=\"done\" style=\"display:none;\">\n\
            <p><b>All done!</b></p>\n\
            <p>The device is now booting with the provided settings and should start receiving commands in a few seconds.</p>\n\
          </div>\n\
          <form method='get' action='setup' id=\"credentials\"  style=\"display:none;\">\n\
            <input id=\"SSID\" type=\"hidden\" name=\"ssid\" />\n\
            <div id=\"SSIDText\"></div></br>\n\
            <div >\n\
              <input id=\"PASSWORD\" type=\"password\" placeholder=\"wireless password\" name=\"pass\"/>\n\
            </div></br>\n\
            <div>\n\
              <input id=\"APIKEY\" type=\"text\" placeholder=\"API Key\" name=\"apikey\"/>\n\
            </div></br>\n\
            <input type=\"button\" id=\"checkWifiButton\" class=\"submit\" value=\"Submit\" onClick=\"checkWiFi()\"/>\
            <p style=\"display:none;\" id=\"buttonBackID\"><input type=\"button\" id=\"backButton\" value=\"Back\" onclick=\"showList()\"/></p>\n\
          </form>\n\
        </div>\n\
      </body>\n\
  </html>";
  server.send(200, "text/html", s);
}

void handleNotFound(void) {
  server.send(404, "text/html", "Nope dude..");
}

void handleCheckWiFi(void) {
  byte retries = 0;
  String qsid = server.arg("ssid");
  qsid.trim();
  Debug2("New SSID: ", qsid);

  String qpass = server.arg("pass");
  qpass.trim();
  Debug2("New Password: ", qpass);

  //we need to disconnect from the STA settings if connected
  //that will disrupt the current HTTP and we need to inform the client
  //to attempt another call
  if (WiFi.isConnected()) {
    Debug("Disconnecting from STA");
    server.send(200, "application/json", "{\"retry\":true, \"reason\":\"Disconnecting WiFi\"}");
    WiFi.disconnect();
    return;
  }

  WiFi.begin(qsid.c_str(), qpass.c_str());
  //testing WiFi
  while ( retries < 20 && 
    WiFi.status() != WL_CONNECTED && 
    WiFi.status() != WL_CONNECT_FAILED) {
    
    delay(1000);

    Debug2("Wifi Status: ", WiFi.status());
    retries++;
  }

  if (WiFi.status() == WL_CONNECTED) {
    Debug("Sendign connected: true");
    server.send(200, "application/json", "{\"connected\":true}");
  } else {
    Debug("Sendign connected: false");
    //TODO: in a later version try to figure out reasons and pass it to the client
    server.send(200, "application/json", "{\"connected\":false, \"reason\":\"unknown\"}");
    
    Debug("Disconnecting from STA");
    WiFi.disconnect();
  }
}

void handleCheckAPI(void) {
  String apikey = server.arg("apikey");
  apikey.trim();
  Debug2("New API Key: ", apikey);

  bool result = apiClient.getCredentials(apikey);

  if (result) {
    server.send(200, "application/json", "{ \"valid\":true}");
  } else {
    //TODO: this does not show anny connection problems
    server.send(200, "application/json", "{ \"valid\":false}");
  }
}

WiFiSetup::WiFiSetup(void) {
  eepromOffset = 0;
  hasSettings = false;
  
  unsigned char mac[6];
  WiFi.macAddress(mac);
  strcpy(apSSID, "LightFeed-");
  sprintf(apSSID+10, "%02x%02x%02x", mac[3], mac[4], mac[5]);
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

      //we'll skip networks with the same name (corporate networks have multiple APs with same name)
      bool same = false;
      for (int j = 0; j < i; j++) {
        if (WiFi.SSID(i).equals(WiFi.SSID(j))) {
          same = true;
          break;
        }
      }
      if (same) continue;

      st += "<a href=\"#\" onclick=\"populate(this)\" ssid=\"" + WiFi.SSID(i) + "\">" + WiFi.SSID(i) + "</a>\n";
    }
}

//TODO: rename
void WiFiSetup::setupAP(void) {
  //TODO: figure out how to enter both modes
  WiFi.mode(WIFI_AP);
  //we set the ip to this super weird number because the standard 192.168.1.1 
  //can conflict with the AP to which the device is connecting and that will
  //create a problem for the HTTPClient
  //https://github.com/esp8266/Arduino/issues/2081
  IPAddress apIP(100, 100, 100, 100);
  WiFi.softAPConfig(apIP, apIP, IPAddress(255, 255, 255, 0));
  WiFi.softAP(apSSID);
}

bool WiFiSetup::anyConnections(void)  {
  if (WiFi.softAPgetStationNum() > 0) {
    Debug("Station connected");
    return true;
  }
  else return false;
}

/*
 * Loads the SSID, pass an API Key number into wifi
 */
void WiFiSetup::loadStationSettings(void) {
  ssid = "";
  pass = "";
  apiKey = "";

  hasSettings = false;

  Debug("Checking if settings are stored in EEPROM");
  hasSettings = MAGIC_NUMBER == EEPROM.read(eepromOffset + SSID_MAX + PASS_MAX + APIKEY_MAX);

  if (hasSettings) {
    Debug("Reading EEPROM ssid");
    for (int i = 0; i < SSID_MAX; ++i) {
      char readChar = char(EEPROM.read(eepromOffset + i));
      if (readChar == 0) break;
      ssid += readChar;
    }
    Debug2("SSID: ", ssid);

    Debug("Reading EEPROM pass");
    for (int i = SSID_MAX; i < SSID_MAX + PASS_MAX; ++i) {
      char readChar = char(EEPROM.read(eepromOffset + i));
      if (readChar == 0) break;
      pass += readChar;
    }
    Debug2("Password: ", pass);

    Debug("Reading EEPROM API Key");
    for (int i = SSID_MAX + PASS_MAX; i < SSID_MAX + PASS_MAX + APIKEY_MAX; ++i) {
      char readChar = char(EEPROM.read(eepromOffset + i));
      if (readChar == 0) break;
      apiKey += readChar;
    }
    Debug2("Hub: ", apiKey);
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
String WiFiSetup::getApiKey(void) {
  return apiKey;
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
  //WiFi.mode(WIFI_AP_STA);
  Debug("mDNS responder started");

  server.on("/", HTTP_GET, handleRoot);
  server.on("/setup", HTTP_GET, handleSetup);
  server.on("/checkwifi", HTTP_GET, handleCheckWiFi);
  server.on("/checkapi", HTTP_GET, handleCheckAPI);

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
    //we don't need this while running very fast so try to do something else..
    delay(100);
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
  while ( retries < 30 ) {
    if (WiFi.status() == WL_CONNECTED) {

      Debug("Connected to wifi! Local IP:");
      Debug(WiFi.localIP());
      return(true);
    }
    delay(500);
    Debug2("Wifi Status: ", WiFi.status());
    retries++;
  }

  Serial.println("Connect timed out");
  return(false);
}

WiFiSetup wifiSetup = WiFiSetup();
