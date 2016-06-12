#include <Wire.h>
#include <SeeedOLED.h>

#define PIN_FORWARD     8
#define PIN_BACKWARD    9

#define LOOP_MAX    5
#define DRIVE_MSEC  30000
#define REST_MSEC   180000

unsigned long g_splitTime = 0;
int g_motor_state = -1; // -1:first-time 0:forward 1:rest_forward 2:backward 3:rest_backward
int g_loop_num = 0;

void setup() {
  Wire.begin();
  SeeedOled.init();  //initialze SEEED OLED display

  SeeedOled.clearDisplay();          //clear the screen and set start position to top left corner
  SeeedOled.setNormalDisplay();      //Set display to normal mode (i.e non-inverse mode)
  SeeedOled.setPageMode();           //Set addressing mode to Page Mode
  SeeedOled.setTextXY(0,0);          //Set the cursor to Xth Page, Yth Column  
  SeeedOled.putString("Hello World!"); //Print the String

  pinMode(PIN_FORWARD, OUTPUT);
  pinMode(PIN_BACKWARD, OUTPUT);

  // STOP
  digitalWrite(PIN_FORWARD, LOW);
  digitalWrite(PIN_BACKWARD, LOW);

  g_splitTime = millis();

}

void loop() {
  if( g_loop_num >= LOOP_MAX ){
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
        SeeedOled.clearDisplay();
        SeeedOled.putString("FORWARD");
        digitalWrite(PIN_FORWARD, HIGH);
        digitalWrite(PIN_BACKWARD, LOW);
      }else if(motor_state == 2){
        // BACKWARD
        SeeedOled.clearDisplay();
        SeeedOled.putString("BACKWARD");
        digitalWrite(PIN_FORWARD, LOW);
        digitalWrite(PIN_BACKWARD, HIGH);
      }else if(motor_state == 1 || motor_state == 3){
        // REST
        SeeedOled.clearDisplay();
        SeeedOled.putString("REST");
        digitalWrite(PIN_FORWARD, LOW);
        digitalWrite(PIN_BACKWARD, LOW);
        if(motor_state == 3){
          g_loop_num++;
          if(g_loop_num >= LOOP_MAX){
            SeeedOled.clearDisplay();
            SeeedOled.putString("break is over.");
          }
        }
      }else{
        // FIRST
        SeeedOled.clearDisplay();
        SeeedOled.putString("FIRST");
        digitalWrite(PIN_FORWARD, LOW);
        digitalWrite(PIN_BACKWARD, LOW);
      }
      g_motor_state = motor_state;
    }
  } // if( g_loop_num > LOOP_MAX ) else
  delay(0);
}
