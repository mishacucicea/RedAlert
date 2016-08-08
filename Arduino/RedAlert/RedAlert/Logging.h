#define info
#define debug

#ifndef logging_h
#define logging_h



  #ifdef debug
  
    #define Debug(x) Serial.print("DEBG: ");Serial.println(x)
    #define Debug2(x,y) Serial.print("DEBG: ");Serial.print(x);Serial.println(y)
    #define Debug3(x,y,z) Serial.print("DEBG: ");Serial.print(x);Serial.print(y);Serial.println(z)
  
  #else
  
    #define Debug(x) 
    #define Debug2(x,y) 
    #define Debug3(x,y,z) 
  
  #endif

  #ifdef info
  
    #define Info(x) Serial.print("DEBG: ");Serial.println(x)
    #define Info2(x,y) Serial.print("DEBG: ");Serial.print(x);Serial.println(y)
    #define Info3(x,y,z) Serial.print("DEBG: ");Serial.print(x);Serial.print(y);Serial.println(z)
  
  #else
  
    #define Info(x) 
    #define Info2(x,y) 
    #define Info3(x,y,z) 
  
  #endif
  
#endif 
