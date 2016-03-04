//#include <SoftwareSerial.h>

#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>
#include <WiFiClient.h>
#include <EEPROM.h>
//#include <WiFiUdp.h>
//#include "sha256.h"
//#include "Base64.h"
#include "PubSubClient.h"



#define Debug(x) Serial.print("DEBG: ");Serial.println(x)
#define Debug2(x,y) Serial.print("DEBG: ");Serial.print(x);Serial.println(y)
#define Debug3(x,y,z) Serial.print("DEBG: ");Serial.print(x);Serial.print(y);Serial.println(z)

char* deviceSSID = "RedAlert-123";

String ssid = "";
String pass = "";

unsigned long lastTimeCheck = 0;

#define hubName "what name?"
#define hubUser "hub user"
#define hubPass "hub SAS token"
#define hubTopic "some topic?"

//#define SOFTWARE_RX 4
//#define SOFTWARE_TX 3

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

IPAddress hubIP(172, 16, 0, 2); //TODO: set IoT Hub IP
WiFiClient wclient;
PubSubClient client(wclient, hubIP);

//Access Point mode
MDNSResponder mdns;
WiFiServer server(80);

//the html for the list of available stations
String st;

void callback(const MQTT::Publish& pub) {
  // handle message arrived
}

int mdns1(int webtype) {
  // Check for any mDNS queries and send responses
  mdns.update();
  
  // Check if a client has connected
  WiFiClient client = server.available();
  if (!client) {
    return(20);
  }

  Debug("New client");

  // Wait for data from client to become available
  while(client.connected() && !client.available()){
    delay(1);
   }
  
  // Read the first line of HTTP request
  String req = client.readStringUntil('\r');
  
  // First line of HTTP request looks like "GET /path HTTP/1.1"
  // Retrieve the "/path" part by finding the spaces
  int addr_start = req.indexOf(' ');
  int addr_end = req.indexOf(' ', addr_start + 1);
  if (addr_start == -1 || addr_end == -1) {
    Debug("Invalid request: ");
    Debug(req);
    return(20);
  }
  
  req = req.substring(addr_start + 1, addr_end);
  Debug("Request: ");
  Debug(req);
  client.flush(); 
  
  String s;
  if ( webtype == 1 ) {
      if (req == "/") {
        IPAddress ip = WiFi.softAPIP();
        String ipStr = String(ip[0]) + '.' + String(ip[1]) + '.' + String(ip[2]) + '.' + String(ip[3]);
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n<!DOCTYPE HTML>\r\n<html>Hello from ESP8266 at ";
        s += ipStr;
        s += "<p>";
        s += st;
        s += "<form method='get' action='a'><label>SSID: </label><input name='ssid' length=32><input name='pass' length=64><input type='submit'></form>";
        s += "</html>\r\n\r\n";
        Debug("Sending 200");
      }
      else if ( req.startsWith("/a?ssid=") ) {
        // /a?ssid=blahhhh&pass=poooo
        Debug("clearing eeprom");
        for (int i = 0; i < 96; ++i) { EEPROM.write(i, 0); }
        
        String qsid; 
        qsid = req.substring(8,req.indexOf('&'));
        Debug(qsid);
        
        String qpass;
        qpass = req.substring(req.lastIndexOf('=')+1);
        Debug(qpass);
        
        Debug("writing eeprom ssid:");
        for (int i = 0; i < qsid.length(); ++i) {
            EEPROM.write(i, qsid[i]);
        }
       
        Debug("writing eeprom pass:"); 
        for (int i = 0; i < qpass.length(); ++i) {
            EEPROM.write(32+i, qpass[i]);; 
        }    
        EEPROM.commit();
        
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n<!DOCTYPE HTML>\r\n<html>Hello from ESP8266 ";
        s += "Found ";
        s += req;
        s += "<p> saved to eeprom... reset to boot into new wifi</html>\r\n\r\n";
      }
      else {
        s = "HTTP/1.1 404 Not Found\r\n\r\n";
        Debug("Sending 404");
      }
  } 
  else {
      if (req == "/")
      {
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n<!DOCTYPE HTML>\r\n<html>Hello from ESP8266";
        s += "</html>\r\n\r\n";
        Debug("Sending 200");
      }
      else if ( req.startsWith("/cleareeprom") ) {
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n<!DOCTYPE HTML>\r\n<html>Hello from ESP8266";
        s += "<p>Clearing the EEPROM<p>";
        s += "</html>\r\n\r\n";
        Debug("Sending 200");  
        Debug("clearing eeprom");
        for (int i = 0; i < 96; ++i) { EEPROM.write(i, 0); }
        EEPROM.commit();
      }
      else
      {
        s = "HTTP/1.1 404 Not Found\r\n\r\n";
        Debug("Sending 404");
      }       
  }
  client.print(s);
  Debug("Done with client");
  
  return(20);
}

void launchWeb(int webtype) {

  if (!mdns.begin("esp8266", WiFi.localIP())) {
    Debug("Error setting up MDNS responder!");

    //TODO: figure out what to do, otherwise it will just hang..
    while(1) { 
      delay(1000);
    }
  }
  
  Debug("mDNS responder started");
  // Start the server
  server.begin();
  Debug("Server started");   
  
  int b = 20;
  while(b == 20) { 
     b = mdns1(webtype);
   }
}

void setupAP(void) {
  Debug("Switching STA mode (Station)");
  WiFi.mode(WIFI_STA);
  
  Debug("Disconnecting");
  WiFi.disconnect();
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
      Debug(String(WiFi.SSID(i)) + " (" + WiFi.RSSI(i) + ")" + (WiFi.encryptionType(i) == ENC_TYPE_NONE)?" ":"*");
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
  delay(100);

  Debug("Start the access point");
  WiFi.softAP(deviceSSID);

  //launchWeb(1);
  Debug("over");
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

void setup() {
  //need for debugging and communication with the slave module
  Serial.begin(115200);

  //start the eeprom and wait 10 msecs for safety
  EEPROM.begin(512);
  delay(10);

  Debug("Reading EEPROM ssid");
  //String esid = "";
  for (int i = 0; i < 32; ++i) {
      ssid += char(EEPROM.read(i));
  }
  Debug2("SSID: ", ssid);
  
  Debug("Reading EEPROM pass");
  //String epass = "";
  for (int i = 32; i < 96; ++i)
    {
      pass += char(EEPROM.read(i));
    }
  Debug2("PASS: ", pass);

  if ( ssid.length() > 1 ) {
      // test esid 
      WiFi.begin(ssid.c_str(), pass.c_str());
      if ( testWifi() == 20 ) { 
          launchWeb(0);
          return;
      }
  }
  
  //TODO: setupAP needs to be done also in case the reset button is pressed
  setupAP(); 
}

void loop() {

  unsigned long now = millis();
  //at start, or once each day
  if (lastTimeCheck == 0 || now - lastTimeCheck > 24*3600*1000) {
    lastTimeCheck = now;
    
    //TODO: check for time agains google
    //do the wifi client and shit
  }

  if (WiFi.status() != WL_CONNECTED) {
    //TODO: debug info
    WiFi.begin(ssid.c_str(), pass.c_str());

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



  if (client.connected()) {
    
    if (!client.loop()) Debug("MQTT failed to loop");
  }
}

