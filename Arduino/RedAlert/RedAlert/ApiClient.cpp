#include "ApiClient.h"

#define PRIMARYAPI "http://lightfeed.eu/api/iot/authentication?devicekey="
//TODO: setup a secondary API URL
#define SECONDARYAPI "http://lightfeed.eu/api/iot/authentication?devicekey="

ApiClient::ApiClient(void) {
  //TOOD: figure out how to null the strings
}

bool ApiClient::getCredentials(String apiKey) {
  //TODO: check for time agains google
  //do the wifi client and shit
  
  HTTPClient http;
  int httpCode = 0;
  
  {
    String s = PRIMARYAPI;
    s += apiKey;
    //try PRIMARY API URL
    Debug2("Making HTTP request to:", s);
    http.begin(s);
    http.setTimeout(30000);
    httpCode = http.GET();
  }
  //try SECONDARY API URL if primary failed
  if (httpCode != HTTP_CODE_OK) {
    String s2 = SECONDARYAPI;
    s2 += apiKey;
    
    Debug2("Making Secondary HTTP request to:", s2);
    http.begin(s2);
    http.setTimeout(30000);
    httpCode = http.GET();  
  }

  if (httpCode > 0) {
    if (httpCode == HTTP_CODE_OK) {
      Debug("Getting authetnication data");
      String payload = http.getString();
      //now we have to split the payload
      int index = payload.indexOf('\n');
      String hubAddress_ = payload.substring(0, index);
      hubAddress_.trim();
      hubAddress_.toCharArray(_hubAddress, hubAddress_.length() + 1);
      payload.remove(0, index+1);

      index = payload.indexOf('\n');
      String deviceId_ = payload.substring(0, index);
      deviceId_.trim();
      deviceId_.toCharArray(_deviceId, deviceId_.length() + 1);
      payload.remove(0, index+1);

      index = payload.indexOf('\n');
      String hubUser_ = payload.substring(0, index);
      hubUser_.trim();
      hubUser_.toCharArray(_hubUser, hubUser_.length() + 1);
      payload.remove(0, index+1);

      index = payload.indexOf('\n');
      String hubPass_ = payload.substring(0, index);
      hubPass_.trim();
      hubPass_.toCharArray(_hubPass, hubPass_.length() + 1);
      payload.remove(0, index+1);

      //what's left is the hubTopic
      index = payload.indexOf('\n');
      if (index == -1) {
        payload.trim();
        payload.toCharArray(_hubTopic, payload.length() + 1);
      } else {
        String hubTopic_ = payload.substring(0, index);
        hubTopic_.trim();
        hubTopic_.toCharArray(_hubTopic, hubTopic_.length() + 1);
        payload.remove(0, index+1);
        
        //also that means that we have details for the second hub
        _hasSecondary = true;
        
        hubAddress_ = payload.substring(0, index);
        hubAddress_.trim();
        hubAddress_.toCharArray(_hubAddress2, hubAddress_.length() + 1);
        payload.remove(0, index+1);

        index = payload.indexOf('\n');
        deviceId_ = payload.substring(0, index);
        deviceId_.trim();
        deviceId_.toCharArray(_deviceId2, deviceId_.length() + 1);
        payload.remove(0, index+1);

        index = payload.indexOf('\n');
        hubUser_ = payload.substring(0, index);
        hubUser_.trim();
        hubUser_.toCharArray(_hubUser2, hubUser_.length() + 1);
        payload.remove(0, index+1);

        index = payload.indexOf('\n');
        hubPass_ = payload.substring(0, index);
        hubPass_.trim();
        hubPass_.toCharArray(_hubPass2, hubPass_.length() + 1);
        payload.remove(0, index+1);
        
        payload.trim();
        payload.toCharArray(_hubTopic2, payload.length() + 1);
      }
      
      //just for testing:
      Debug2("hubAddress: ", _hubAddress);
      Debug2("deviceId: ", _deviceId);
      Debug2("hubUser: ", _hubUser);
      Debug2("hubPass: ", _hubPass);
      Debug2("hubTopic: ", _hubTopic);
      
      return true;
    }
    else {
      //TODO: what do we do?
      Debug2("Error status code", httpCode); 
      return false;
    }
  }
  else {
    //TODO: an error occurred, what do we do?
    Debug2("Failed to GET: ", httpCode);
    return false;
  }
}

bool ApiClient::hasSecondary(void) {
  return _hasSecondary;
}

char* ApiClient::getHubAddress(void) {
  return _hubAddress;
}

char* ApiClient::getDeviceId(void) {
  return _deviceId;
}

char* ApiClient::getHubUser(void) {
  return _hubUser;
}

char* ApiClient::getHubPass(void) {
  return _hubPass;
}

char* ApiClient::getHubTopic(void) {
  return _hubTopic;
}

char* ApiClient::getHubAddress2(void) {
  return _hubAddress2;
}

char* ApiClient::getDeviceId2(void) {
  return _deviceId2;
}

char* ApiClient::getHubUser2(void) {
  return _hubUser2;
}

char* ApiClient::getHubPass2(void) {
  return _hubPass2;
}

char* ApiClient::getHubTopic2(void) {
  return _hubTopic2;
}

ApiClient apiClient = ApiClient();
