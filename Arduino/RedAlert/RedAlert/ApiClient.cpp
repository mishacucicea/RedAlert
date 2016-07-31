#include "ApiClient.h"


/*
struct ApiCredentials {
  String hubAddress;
  String deviceId;
  String hubUser;
  String hubPass;
  String hubTopic;
};
*/

ApiClient::ApiClient(void) {
  //TOOD: figure out how to null the strings
}

bool ApiClient::getCredentials(String apiKey) {
  //TODO: check for time agains google
  //do the wifi client and shit
  String s = "http://redalertxfd.azurewebsites.net/api/iot/authentication?devicekey=";
  s += apiKey;
  
  Debug2("Making HTTP request to:", s);
  
  HTTPClient http;
  http.begin(s);
  http.setTimeout(30000);
  int httpCode = http.GET();

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
      payload.trim();
      payload.toCharArray(_hubTopic, payload.length() + 1);

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

ApiClient apiClient = ApiClient();
