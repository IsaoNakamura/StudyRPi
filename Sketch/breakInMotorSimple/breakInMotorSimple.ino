#define PIN_FORWARD     8
#define PIN_BACKWARD    9

#define LOOP_MAX    5

#define DRIVE_MSEC    30000
#define PAUSE_MSEC   180000
#define HIDEN_MSEC     1000

unsigned long g_splitTime = 0;
enum motor_state { STOP, FORWARD, PAUSE_FORWARD, BACKWARD, PAUSE_BACKWARD };
motor_state g_motor_state = -1; // -1:stop 0:forward 1:pause_forward 2:backward 3:pause_backward
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
  motor_state motor_state = g_motor_state;
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
  const motor_state motor_state,
  const bool isChanged,
  const unsigned long timeInterval,
  const int loop_num
)
{
  if(motor_state == FORWARD ){
    // FORWARD
    if(isChanged){
      // MOTOR-FORWARD
      digitalWrite(PIN_FORWARD, HIGH);
      digitalWrite(PIN_BACKWARD, LOW);
    }
  }else if(motor_state == BACKWARD){
    // BACKWARD
    if(isChanged){
      // MOTOR-BACKWARD
      digitalWrite(PIN_FORWARD,  LOW);
      digitalWrite(PIN_BACKWARD, HIGH);
    }
  }else if(motor_state == PAUSE_FORWARD || motor_state == 3){
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
  motor_state& motor_state,
  int& loop_num,
  const motor_state prev_motor_state,
  const unsigned long curTime,
  const unsigned long timeInterval
) 
{
  bool bRet = false;

  // this frame motor_stae
  motor_state = prev_motor_state;
  
  // calc current motor_state
  if(prev_motor_state==STOP){
    if( timeInterval > HIDEN_MSEC){
      bRet = true;

      // stop to forward
      motor_state = FORWARD;
    }
  }else 
  if(prev_motor_state==FORWARD || prev_motor_state==BACKWARD){ // 0:forward 2:backward
    if( timeInterval > DRIVE_MSEC){
      bRet = true;

      if(prev_motor_state == FORWARD ){
        // forward to pause
        motor_state = PAUSE_FORWARD;
      }else{
        // backward to pause
        motor_state = 3;
        loop_num++;
      }
    }
  }else if(prev_motor_state==PAUSE_FORWARD || prev_motor_state==3){ // 1:pause_forward or 3:pause_backward
    if( timeInterval > PAUSE_MSEC){
      bRet = true;

      if(prev_motor_state == PAUSE_FORWARD ){
        // pause to backward
        motor_state = BACKWARD;
      }else{
        if( loop_num >= LOOP_MAX ){
          // pause to stop
          motor_state = STOP;
          // loop_num = 0;
        }else{
          // pause to forward
          motor_state = FORWARD;
        }
      }
    }
  }
  return bRet;
}

