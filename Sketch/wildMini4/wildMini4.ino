#include <Servo.h>

Servo myservo;
int preServoYaw = 90;
int preMotorAccel = 0;

void setup() {
  // put your setup code here, to run once:
   // start serial port at 9600 bps:
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }

  myservo.attach( 3 );
  myservo.write(preServoYaw);

  pinMode(8, OUTPUT);
  pinMode(9, OUTPUT);

}

void loop() {
  // put your main code here, to run repeatedly:
  // if we get a valid byte, read analog ins:
  if (Serial.available() > 3) {
    // get incoming byte:
    int inByte = Serial.read();

    if(inByte == 0x7E ){ // 開始デリミタ取得
      //digitalWrite(9, HIGH);
      // データ格納数取得
      int readBuf[3] = {0x0};
      for(int i=0; i<3; i++){
        readBuf[i] = Serial.read();
      }
      if( preServoYaw != readBuf[1] ){
        myservo.write(readBuf[1]);
        preServoYaw = readBuf[1];
      }
      if( preMotorAccel != readBuf[2] ){
        if(readBuf[2] == 90 ){
          // STOP
          analogWrite(8, 0);
          analogWrite(9, 0);
        }else if( (readBuf[2] > 90) && (readBuf[2] <= 180) ){
          // FORWARD
          analogWrite(8, readBuf[2]);
          analogWrite(9, 0);
        }else if( (readBuf[2] < 90) && (readBuf[2] >= 0 ) ){
          // BACK
          analogWrite(8, 0);
          analogWrite(9, readBuf[2]);
        }
        preMotorAccel = readBuf[2];
      }
    }
    // delay(10);
  }
}
