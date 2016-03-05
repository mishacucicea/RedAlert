void setup() {
  // put your setup code here, to run once:
  // initialize digital pin 9 as an output.
  pinMode(9, OUTPUT);
  pinMode(10, OUTPUT);
  pinMode(11, OUTPUT);
  Serial.begin(19200);
}

void loop() {
  // put your main code here, to run repeatedly:
  String color = Serial.readStringUntil('\n');
  int pin = 0;
 
 switch(color[0])
 {
    case 'r':
      pin = 9;
      break;
   case 'y':
      pin = 10;
      break;
   case 'g':
     pin = 11;
     break;
    case 'x':
     pin = -1;
     break;
  }
 
  if(pin != 0)
  {
    digitalWrite(9, LOW);    // turn the LED off by making the voltage LOW
    digitalWrite(10, LOW);    // turn the LED off by making the voltage LOW
    digitalWrite(11, LOW);    // turn the LED off by making the voltage LOW
  }

  if(pin > 0)
  {
    analogWrite(pin, 125);
  }
}
