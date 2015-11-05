#include <Servo.h>

Servo myservo;

int pos = 0;

void setup() {
  // put your setup code here, to run once:
  //pinMode(3, OUTPUT);
  myservo.attach( 3 );
}

void loop() {
  // put your main code here, to run repeatedly:
  //analogWrite( 3, 20 );
  for( pos=50; pos<130; pos++ ){
    myservo.write(pos);
    delay(15);
  }
  for( pos=130; pos>50; pos-- ){
    myservo.write(pos);
    delay(15);
  }
}
