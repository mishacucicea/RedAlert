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
  private:
    char _hubAddress[100];
    char _deviceId[100];
    char _hubUser[100];
    char _hubPass[256];
    char _hubTopic[100];
};

extern ApiClient apiClient;
#endif