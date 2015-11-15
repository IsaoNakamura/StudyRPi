void setup() {
  // put your setup code here, to run once:
   // start serial port at 9600 bps:
  Serial.begin(9600);
  while (!Serial) {
    ; // wait for serial port to connect. Needed for Leonardo only
  }

  pinMode(9, OUTPUT);
  // establishContact();  // send a byte to establish contact until receiver responds

}

void loop() {
  // put your main code here, to run repeatedly:
  // if we get a valid byte, read analog ins:
  if (Serial.available() > 0) {
    // get incoming byte:
    int inByte = Serial.read();

    if(inByte == 0x30 ){ // 0
      digitalWrite(9, LOW);
    }else{
      digitalWrite(9, HIGH);
    }
    delay(10);
  }
}

void establishContact() {
  while (Serial.available() <= 0) {
    Serial.print('A');   // send a capital A
    delay(300);
  }
}
