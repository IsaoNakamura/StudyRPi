#define PIN_FORWARD     8
#define PIN_BACKWARD    9

#define LOOP_MAX    4
#define DRIVE_MSEC  100
#define REST_MSEC   3000

unsigned long g_splitTime = 0;
int g_motor_state = -1; // -1:first-time 0:forward 1:rest_forward 2:backward 3:rest_backward
int g_loop_num = 0;

void setup() {
  // put your setup code here, to run once:

  pinMode(PIN_FORWARD, OUTPUT);
  pinMode(PIN_BACKWARD, OUTPUT);

  // STOP
  digitalWrite(PIN_FORWARD, LOW);
  digitalWrite(PIN_BACKWARD, LOW);

  g_splitTime = millis();

}

void loop() {
  if( g_loop_num > LOOP_MAX ){
    // STOP
    digitalWrite(PIN_FORWARD, LOW);
    digitalWrite(PIN_BACKWARD, LOW);
  }else{
     // get current time.
    unsigned long curTime = millis();
    int motor_state = g_motor_state;
  
    if(curTime > g_splitTime){
      unsigned long timeInterval = curTime - g_splitTime;
    
      // calc current motor_state
      if(g_motor_state==-1){
        g_splitTime = curTime;
        // first to forward
        motor_state = 0;
      }else 
      if(g_motor_state==0 || g_motor_state==2){
        if( timeInterval > DRIVE_MSEC){
          g_splitTime = curTime;
          if(g_motor_state == 0 ){
            // forward to rest
            motor_state = 1;
          }else{
            // backward to rest
            motor_state = 3;
          }
        }
      }else if(g_motor_state==1 || g_motor_state==3){
        if( timeInterval > REST_MSEC){
          g_splitTime = curTime;
          if(g_motor_state == 1 ){
            // rest to backward
            motor_state = 2;
          }else{
            // rest to forward
            motor_state = 0;
          }
        }
      }
    }
  
    if(g_motor_state != motor_state){
      if(motor_state == 0 ){
        // FORWARD
        digitalWrite(PIN_FORWARD, HIGH);
        digitalWrite(PIN_BACKWARD, LOW);
        g_loop_num++;
        
      }else if(motor_state == 2){
        // BACKWARD
        digitalWrite(PIN_FORWARD, LOW);
        digitalWrite(PIN_BACKWARD, HIGH);
      }else if(motor_state == 1 || motor_state == 3){
        // REST
        digitalWrite(PIN_FORWARD, LOW);
        digitalWrite(PIN_BACKWARD, LOW);
      }else{
        // FIRST
        digitalWrite(PIN_FORWARD, LOW);
        digitalWrite(PIN_BACKWARD, LOW);
      }
      g_motor_state = motor_state;
    }
  } // if( g_loop_num > LOOP_MAX ) else
  delay(0);
}
