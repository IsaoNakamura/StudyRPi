#include <Servo.h>

Servo myservo;

int pos = 0;

void setup() {
  // put your setup code here, to run once:
  //pinMode(3, OUTPUT);
  myservo.attach( 10 );
  myservo.write(65);
}

void loop() {
  // put your main code here, to run repeatedly:
  //analogWrite( 3, 20 );
  for( pos=65; pos<130; pos++ ){
    myservo.write(pos);
    delay(15);
  }
  for( pos=130; pos>65; pos-- ){
    myservo.write(pos);
    delay(15);
  }
  myservo.write(65);
  delay(2000);
}
