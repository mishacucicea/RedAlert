#ifndef ApiClient_h
#define ApiClient_h

#include "Arduino.h"
#include "Logging.h"
#include <ESP8266HTTPClient.h>


class ApiClient {
  public:
    ApiClient(void);
    bool getCredentials(String apiKey);
    
    char* getHubAddress(void);
    char* getDeviceId(void);
    char* getHubUser(void);
    char* getHubPass(void);
    char* getHubTopic(void);
    
    char* getHubAddress2(void);
    char* getDeviceId2(void);
    char* getHubUser2(void);
    char* getHubPass2(void);
    char* getHubTopic2(void);
    
    bool hasSecondary(void);
  private:
    char _hubAddress[100];
    char _deviceId[100];
    char _hubUser[100];
    char _hubPass[256];
    char _hubTopic[100];
    
    char _hubAddress2[100];
    char _deviceId2[100];
    char _hubUser2[100];
    char _hubPass2[256];
    char _hubTopic2[100];
    
    bool _hasSecondary = false;
};

extern ApiClient apiClient;
#endif