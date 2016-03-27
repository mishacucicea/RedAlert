//#include <SoftwareSerial.h>

#include <ESP8266WiFi.h>
#include <ESP8266mDNS.h>

#include <WiFiClient.h>
#include <EEPROM.h>
//#include <WiFiUdp.h>
//#include "sha256.h"
//#include "Base64.h"
#include "PubSubClient.h"

#define debug


#define Debug(x) Serial.print("DEBG: ");Serial.println(x)
#define Debug2(x,y) Serial.print("DEBG: ");Serial.print(x);Serial.println(y)
#define Debug3(x,y,z) Serial.print("DEBG: ");Serial.print(x);Serial.print(y);Serial.println(z)

//TODO: randomize the device SSID
char* deviceSSID = "RedAlert-123";

String ssid = "";
String pass = "";
//serial number
String sn = "";

unsigned long lastTimeCheck = 0;

//maximum length of the specific EEPROM values
#define SSID_MAX 32
#define PASS_MAX 64
#define SERIAL_MAX 36

#define hubAddress "RedAlertHubArduino.azure-devices.net"
#define hubName "what name?"
#define hubUser "hub user"
#define hubPass "hub SAS token"
#define hubTopic "some topic?"

#define Red 1
#define Green 2
#define Yellow 3

//define pins
#define RED_PIN 14
#define GREEN_PIN 12
#define YELLOW_PIN 13

//#define SOFTWARE_RX 4
//#define SOFTWARE_TX 3

//ref: https://github.com/oh1ko/ESP82666_OLED_clock/blob/master/ESP8266_OLED_clock.ino

//ref: https://github.com/chriscook8/esp-arduino-apboot/blob/master/ESP-wifiboot.ino

WiFiClient wclient;
PubSubClient client(wclient);

//Access Point mode
MDNSResponder mdns;
WiFiServer server(80);

//the html for the list of available stations
String st;

void setColor(int color) {
  if (color == Yellow) analogWrite(YELLOW_PIN, 1024/2);
  else analogWrite(YELLOW_PIN, 0);

  if (color == Green) analogWrite(GREEN_PIN, 1024/2);
  else analogWrite(GREEN_PIN, 0);

  if (color == Red) analogWrite(RED_PIN, 1024/2);
  else analogWrite(RED_PIN, 1024/2);
}

void callback(char* topic, byte* payload, unsigned int length) {
  // handle message arrived
  String msg = "";
  for (int i = 0; i < length; i++) {
    msg += (char)payload[i];
  }

  if (msg.startsWith("RED")) {
    setColor(Red);
  } else if (msg.startsWith("GRN")) {
    setColor(Green);
  } else {
    setColor(Yellow);
  }
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
        String ipStr = String(ip[0]) + '.' + String(ip[1]) + '.' + 
          String(ip[2]) + '.' + String(ip[3]);
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n\
<!DOCTYPE HTML>\r\n\
<html>Hello from ESP8266 at ";
        s += ipStr;
        s += "<p>";
        s += st;
        s += "\
  <form method='get' action='a'>\
    <label>SSID: </label>\
    <input name='ssid' length=32>\
    <input name='pass' length=64>\
    <input name='serial' length=36>\
    <input type='submit'>\
  </form>\
</html>\r\n\r\n";
        Debug("Sending setup form");
      }
      else if ( req.startsWith("/a?ssid=") ) {
        // /a?ssid=blahhhh&pass=poooo
        Debug("clearing eeprom");
        for (int i = 0; i < SSID_MAX + PASS_MAX + SERIAL_MAX; ++i) { EEPROM.write(i, 0); }
        
        String qsid; 
        //TODO: fix it!
        qsid = req.substring(8,req.indexOf('&'));
        Debug2("New SSID: ", qsid);
        
        req = req.substring(req.indexOf('&') + 1);
        
        String qpass;
        qpass = req.substring(req.indexOf('=') + 1, req.indexOf('&'));
        Debug2("New Password: ", qpass);
        
        req = req.substring(req.indexOf('&') + 1);
        
        //serial number
        String qsn;
        qsn = req.substring(req.indexOf('=') + 1);
        Debug2("New Serial Number: ", qsn);
        
        Debug("writing eeprom ssid");
        for (int i = 0; i < qsid.length(); ++i) {
            EEPROM.write(i, qsid[i]);
        }
       
        Debug("writing eeprom pass"); 
        for (int i = 0; i < qpass.length(); ++i) {
            EEPROM.write(SSID_MAX + i, qpass[i]);
        }    

        Debug("wrigin eeprom serial number");
        for (int i = 0; i < qsn.length(); ++i) {
          EEPROM.write(SSID_MAX + PASS_MAX + i, qsn[i]);
        }
        
        EEPROM.commit();
        
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n\
<!DOCTYPE HTML>\r\n<html>Hello from ESP8266 ";
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
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n\
<!DOCTYPE HTML>\r\n<html>Hello from ESP8266";
        s += "</html>\r\n\r\n";
        Debug("Sending 200");
      }
      else if ( req.startsWith("/cleareeprom") ) {
        s = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\n\r\n\
<!DOCTYPE HTML>\r\n<html>Hello from ESP8266";
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

  Debug("Waiting for Wifi ready");

  
  Debug2("Assigned IP: ", WiFi.softAPIP());//WiFi.localIP());
  if (!mdns.begin("esp8266", WiFi.softAPIP())) {
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
  delay(100);

  Debug("Start the access point");

  
  WiFi.mode(WIFI_AP);
  IPAddress apIP(192, 168, 1, 1);
  WiFi.softAPConfig(apIP, apIP, IPAddress(255, 255, 255, 0));
  WiFi.softAP(deviceSSID);

  launchWeb(1);
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

void getDeviceId(void) {
  
}

void getSAS(void) {
  
}

void setup() {
  //need for debugging and communication with the slave module
  Serial.begin(74880);

  pinMode(RED_PIN, OUTPUT);
  pinMode(GREEN_PIN, OUTPUT);
  pinMode(YELLOW_PIN, OUTPUT);

  delay(5000);

  //start the eeprom and wait 10 msecs for safety
  EEPROM.begin(512);
  delay(10);

  Debug("Reading EEPROM ssid");
  for (int i = 0; i < SSID_MAX; ++i) {
      ssid += char(EEPROM.read(i));
  }
  Debug2("SSID: ", ssid);
  
  Debug("Reading EEPROM pass");
  for (int i = SSID_MAX; i < SSID_MAX + PASS_MAX; ++i) {
      pass += char(EEPROM.read(i));
  }
  Debug2("Password: ", pass);

  Debug("Reading EEPROM Serial Number");
  for (int i = SSID_MAX + PASS_MAX; i < SSID_MAX + PASS_MAX + SERIAL_MAX; ++i) {
    sn += char(EEPROM.read(i));
  }
  Debug2("Hub: ", sn);

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

  //this will set the address of the hub and port on which it communicates
  client.setServer(hubAddress, 8883);
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
      if (client.connect(hubName, hubUser, hubPass)) {
        //TODO: log connected status
      }

      client.setCallback(callback);
      //client.publish("outTopic", "test");
      client.subscribe(hubTopic);
    }
  }

  if (client.connected()) {
    
    if (!client.loop()) Debug("MQTT failed to loop");
  }
}

