#define PIN_FORWARD     8
#define PIN_BACKWARD    9

#define LOOP_MAX    5

#define DRIVE_MSEC    30000
#define PAUSE_MSEC   180000
#define HIDEN_MSEC     1000

unsigned long g_splitTime = 0;
int g_motor_state = -1; // -1:stop 0:forward 1:pause_forward 2:backward 3:pause_backward
int g_loop_num = 0;

void setup() {
  pinMode(PIN_FORWARD, OUTPUT);
  pinMode(PIN_BACKWARD, OUTPUT);

  // STOP
  digitalWrite(PIN_FORWARD, LOW);
  digitalWrite(PIN_BACKWARD, LOW);

  g_splitTime = millis();
}

void loop() {
  // get current time.
  unsigned long curTime = millis();
  unsigned long timeInterval = 0;
  if(curTime >= g_splitTime){
    timeInterval = curTime - g_splitTime;
  }

  // calc current motor_state
  int motor_state = g_motor_state;
  bool isChanged = calcCurrentMotorState(motor_state, g_loop_num, g_motor_state, curTime, timeInterval);
  if( isChanged ){
    g_splitTime = curTime;
  }

  // action by motor_state.
  actionMotor(motor_state, isChanged, timeInterval, g_loop_num);

  // update g_motor_state.
  if( isChanged ){
    g_motor_state = motor_state;
  }

  delay(0);
}

void actionMotor
(
  const int motor_state,
  const bool isChanged,
  const unsigned long timeInterval,
  const int loop_num,
)
{
  if(motor_state == 0 ){
    // FORWARD
    if(isChanged){
      // MOTOR-FORWARD
      digitalWrite(PIN_FORWARD, HIGH);
      digitalWrite(PIN_BACKWARD, LOW);
    }
  }else if(motor_state == 2){
    // BACKWARD
    if(isChanged){
      // MOTOR-BACKWARD
      digitalWrite(PIN_FORWARD,  LOW);
      digitalWrite(PIN_BACKWARD, HIGH);
    }
  }else if(motor_state == 1 || motor_state == 3){
    // PAUSE
    if(isChanged){
      // MOTOR-STOP
      digitalWrite(PIN_FORWARD, LOW);
      digitalWrite(PIN_BACKWARD, LOW);
    }
  }else{
    // STOP
    if(isChanged){
      // MOTOR-STOP
      digitalWrite(PIN_FORWARD, LOW);
      digitalWrite(PIN_BACKWARD, LOW);
    }
  }
  return;
}

bool calcCurrentMotorState
(
  int& motor_state,
  int& loop_num,
  const int prev_motor_state,
  const unsigned long curTime,
  const unsigned long timeInterval
) 
{
  bool bRet = false;

  // this frame motor_stae
  motor_state = prev_motor_state;
  
  // calc current motor_state
  if(prev_motor_state==-1){ // -1:stop
    if( timeInterval > HIDEN_MSEC){
      bRet = true;

      // stop to forward
      motor_state = 0;
    }
  }else 
  if(prev_motor_state==0 || prev_motor_state==2){ // 0:forward 2:backward
    if( timeInterval > DRIVE_MSEC){
      bRet = true;

      if(prev_motor_state == 0 ){
        // forward to pause
        motor_state = 1;
      }else{
        // backward to pause
        motor_state = 3;
        loop_num++;
      }
    }
  }else if(prev_motor_state==1 || prev_motor_state==3){ // 1:pause_forward or 3:pause_backward
    if( timeInterval > PAUSE_MSEC){
      bRet = true;

      if(prev_motor_state == 1 ){
        // pause to backward
        motor_state = 2;
      }else{
        if( loop_num >= LOOP_MAX ){
          // pause to stop
          motor_state = -1;
          // loop_num = 0;
        }else{
          // pause to forward
          motor_state = 0;
        }
      }
    }
  }
  return bRet;
}

void moveServoSmoothly
(
  Servo& servo,
  const int beg_pos,
  const int end_pos,
  const int move_delta,
  const int delay_time
)
{
  int pos = beg_pos;
  servo.write(pos);
  delay(delay_time);

  if(beg_pos <= end_pos){
    while(1){
      pos+=move_delta;
      if(pos>end_pos){
        pos=end_pos;
      }
      servo.write(pos);
      delay(delay_time);
      if(pos>=end_pos){
        break;
      }
    }
  }else{
    while(1){
      pos-=move_delta;
      if(pos<end_pos){
        pos=end_pos;
      }
      servo.write(pos);
      delay(delay_time);
      if(pos<=end_pos){
        break;
      }
    }
  }
  return;
}
