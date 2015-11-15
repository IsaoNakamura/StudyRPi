#include <Servo.h>

Servo myservo;

void setup() {
  // put your setup code here, to run once:
   // start serial port at 9600 bps:
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }

  myservo.attach( 3 );
  pinMode(9, OUTPUT);
}

void loop() {
  // put your main code here, to run repeatedly:
  // if we get a valid byte, read analog ins:
  if (Serial.available() > 3) {
    // get incoming byte:
    int inByte = Serial.read();

    if(inByte == 0x7E ){ // 開始
      digitalWrite(9, HIGH);
      int readBuf[3] = {0x0, 0x0, 0x0};
      for(int i=0; i<3; i++){
        readBuf[i] = Serial.read();
      }
      myservo.write(readBuf[1]);
      //if(readBuf[1]==0x50){
      //  digitalWrite(9, HIGH);
      //  myservo.write(80);
      //}else if(readBuf[1]==0x64){
      //  digitalWrite(9, LOW);
      //  myservo.write(100);
      //}else{
      //   myservo.write(90);
      //}
    }else{
      digitalWrite(9, LOW);
    }
    // delay(10);
  }
}
