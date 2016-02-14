#include <Servo.h>

#define PIN_SERVO   3
#define PIN_ACCEL   5
#define PIN_FORWARD 8
#define PIN_BACK    9

#define SERIAL_BAUDRATE          9600
#define SERIAL_DELIMITER         0x7E
#define SERIAL_FRAMETYPE_MOTOR   0x00
#define SERIAL_NUM               3
#define SERIAL_IDX_TYPE          0
#define SERIAL_IDX_YAW           1
#define SERIAL_IDX_ACCEL         2

Servo myservo;
int defServoYaw = 90;
int defMotorAccel = 0;

void setup() {
  Serial.begin(SERIAL_BAUDRATE);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }

  myservo.attach( PIN_SERVO );
  myservo.write(defServoYaw);

  pinMode(PIN_FORWARD, OUTPUT);
  pinMode(PIN_BACK, OUTPUT);
  // STOP
  digitalWrite(PIN_FORWARD, LOW);
  digitalWrite(PIN_BACK, LOW);
  analogWrite(PIN_ACCEL, defMotorAccel);

}

void loop() {
  if (Serial.available() > SERIAL_NUM) {
    // get incoming byte:
    int inByte = Serial.read();

    if(inByte == SERIAL_DELIMITER ){ // 開始デリミタ取得
      int readBuf[SERIAL_NUM] = {0x0};
      for(int i=0; i<SERIAL_NUM; i++){
        readBuf[i] = Serial.read();
      }
      
      if( readBuf[SERIAL_IDX_TYPE] == SERIAL_FRAMETYPE_MOTOR ){
        // FrameType is Motor-Parameter

        // Ctrl Servo-Motor
        myservo.write(readBuf[SERIAL_IDX_YAW]);
  
        // Ctrl DC-Motor
        if(readBuf[SERIAL_IDX_ACCEL] == 90 ){
          // STOP
          digitalWrite(PIN_FORWARD, LOW);
          digitalWrite(PIN_BACK, LOW);
          analogWrite(PIN_ACCEL, 0);

        }else if( (readBuf[SERIAL_IDX_ACCEL] > 90) && (readBuf[SERIAL_IDX_ACCEL] <= 180) ){
          // FORWARD
          digitalWrite(PIN_FORWARD, HIGH);
          digitalWrite(PIN_BACK, LOW);
          int writeValue = 255 * (readBuf[SERIAL_IDX_ACCEL] - 90) / 90;
          analogWrite(PIN_ACCEL, writeValue);

        }else if( (readBuf[SERIAL_IDX_ACCEL] < 90) && (readBuf[SERIAL_IDX_ACCEL] >= 0 ) ){
          // BACK
          digitalWrite(PIN_FORWARD, LOW);
          digitalWrite(PIN_BACK, HIGH);
          int writeValue = 255 * (90 - readBuf[SERIAL_IDX_ACCEL]) / 90;
          analogWrite(PIN_ACCEL, writeValue);
        }
      }
    }
    // delay(10);
  }
}
