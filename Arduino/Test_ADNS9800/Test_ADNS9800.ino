#include <SPI.h>
#include <Stepper.h>
#include <avr/pgmspace.h>
#define REG_Product_ID 0x00
#define REG_Revision_ID 0x01
#define REG_Motion 0x02
#define REG_Delta_X_L 0x03
#define REG_Delta_X_H 0x04
#define REG_Delta_Y_L 0x05
#define REG_Delta_Y_H 0x06
#define REG_SQUAL 0x07
#define REG_Pixel_Sum 0x08
#define REG_Maximum_Pixel 0x09
#define REG_Minimum_Pixel 0x0a
#define REG_Shutter_Lower 0x0b
#define REG_Shutter_Upper 0x0c
#define REG_Frame_Period_Lower 0x0d
#define REG_Frame_Period_Upper 0x0e
#define REG_Configuration_I 0x0f
#define REG_Configuration_II 0x10
#define REG_Frame_Capture 0x12
#define REG_SROM_Enable 0x13
#define REG_Run_Downshift 0x14
#define REG_Rest1_Rate 0x15
#define REG_Rest1_Downshift 0x16
#define REG_Rest2_Rate 0x17
#define REG_Rest2_Downshift 0x18
#define REG_Rest3_Rate 0x19
#define REG_Frame_Period_Max_Bound_Lower 0x1a
#define REG_Frame_Period_Max_Bound_Upper 0x1b
#define REG_Frame_Period_Min_Bound_Lower 0x1c
#define REG_Frame_Period_Min_Bound_Upper 0x1d
#define REG_Shutter_Max_Bound_Lower 0x1e
#define REG_Shutter_Max_Bound_Upper 0x1f
#define REG_LASER_CTRL0 0x20
#define REG_Observation 0x24
#define REG_Data_Out_Lower 0x25
#define REG_Data_Out_Upper 0x26
#define REG_SROM_ID 0x2a
#define REG_Lift_Detection_Thr 0x2e
#define REG_Configuration_V 0x2f
#define REG_Configuration_IV 0x39
#define REG_Power_Up_Reset 0x3a
#define REG_Shutdown 0x3b
#define REG_Inverse_Product_ID 0x3f
#define REG_Motion_Burst 0x50
#define REG_SROM_Load_Burst 0x62
#define REG_Pixel_Burst 0x64
#define IN1 A2
#define IN2 A3
#define IN3 4
#define IN4 5
#define PhotoUni A0
#define PhotoBi A1
byte initComplete = 0;

const int ncs = 10;

extern const unsigned short firmware_length;
extern const unsigned char firmware_data[];

bool shouldCapture = false;
bool startUni = false;
bool startBi = false;
int halfStepSequence[8][4] = {
  { 1, 0, 0, 0 },
  { 1, 1, 0, 0 },
  { 0, 1, 0, 0 },
  { 0, 1, 1, 0 },
  { 0, 0, 1, 0 },
  { 0, 0, 1, 1 },
  { 0, 0, 0, 1 },
  { 1, 0, 0, 1 }
};
int delayTime = 5;

const int stepsPerRevolution = 200;
Stepper myStepper(stepsPerRevolution, 9, 8, 7, 6);
void setup() {
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT);
  pinMode(IN4, OUTPUT);
  pinMode(PhotoUni, INPUT);
  pinMode(PhotoBi, INPUT);
  myStepper.setSpeed(20);
  Serial.begin(115200);
  while (!Serial);
  pinMode(ncs, OUTPUT);

  SPI.begin();
  SPI.setDataMode(SPI_MODE3);
  SPI.setBitOrder(MSBFIRST);
  SPI.setClockDivider(SPI_CLOCK_DIV8);


  performStartup();
  delay(100);
  initComplete = 9;
}

void adns_com_begin() {
  digitalWrite(ncs, LOW);
}

void adns_com_end() {
  digitalWrite(ncs, HIGH);
}

byte adns_read_reg(byte reg_addr) {
  adns_com_begin();
  SPI.transfer(reg_addr & 0x7f);
  delayMicroseconds(200);
  byte data = SPI.transfer(0);

  delayMicroseconds(1);
  adns_com_end();
  delayMicroseconds(19);

  return data;
}

void adns_write_reg(byte reg_addr, byte data) {
  adns_com_begin();

  SPI.transfer(reg_addr | 0x80);
  SPI.transfer(data);

  delayMicroseconds(20);
  adns_com_end();
  delayMicroseconds(100);
}

void adns_upload_firmware() {

  adns_write_reg(REG_Configuration_IV, 0x02);

  adns_write_reg(REG_SROM_Enable, 0xd);

  delay(10);

  adns_write_reg(REG_SROM_Enable, 0x8);

  adns_com_begin();
  SPI.transfer(REG_SROM_Load_Burst | 0x80);  // write burst destination adress
  delayMicroseconds(15);

  unsigned char c;
  for (int i = 0; i < firmware_length; i++) {
    c = (unsigned char)pgm_read_byte(firmware_data + i);
    SPI.transfer(c);
    delayMicroseconds(15);
  }
  adns_com_end();
}

void performStartup(void) {
  adns_com_begin();                          // ensure that the Serial port is reset
  adns_com_end();                            // ensure that the Serial port is reset
  adns_write_reg(REG_Power_Up_Reset, 0x5a);  // force reset
  delay(50);                                 // wait for it to reboot
  // read registers 0x02 to 0x06 (and discard the data)
  adns_read_reg(REG_Motion);
  adns_read_reg(REG_Delta_X_L);
  adns_read_reg(REG_Delta_X_H);
  adns_read_reg(REG_Delta_Y_L);
  adns_read_reg(REG_Delta_Y_H);
  // upload the firmware
  adns_upload_firmware();
  delay(10);
  //adns_write_reg(REG_Configuration_I, 0x01); // 200 cpi
  adns_write_reg(REG_Configuration_I, 0x09);  // 8200 cpi
  delay(10);
  adns_write_reg(REG_Shutter_Lower, 0x90);
  delay(10);
  adns_write_reg(REG_Shutter_Upper, 0x90);
  delay(10);

  //enable laser(bit 0 = 0b), in normal mode (bits 3,2,1 = 000b)
  // reading the actual value of the register is important because the real
  // default value is different from what is said in the datasheet, and if you
  // change the reserved bytes (like by writing 0x00...) it would not work.

  byte laser_ctrl0 = adns_read_reg(REG_LASER_CTRL0);
  adns_write_reg(REG_LASER_CTRL0, laser_ctrl0 & 0xf0);
  delay(1);
  Serial.println("Szenzor indul....");
}
void sendFrame() {
  adns_write_reg(REG_Frame_Capture, 0x93);
  delayMicroseconds(120);
  adns_write_reg(REG_Frame_Capture, 0xc5);
  delayMicroseconds(120);

  adns_com_begin();
  delayMicroseconds(100);
  byte readys = 0;
  while (readys == 0) {
    SPI.transfer(REG_Motion & 0x7f);
    delayMicroseconds(100);
    readys = SPI.transfer(0);
    readys = readys & 1;
    delayMicroseconds(20);
  }
  SPI.transfer(REG_Pixel_Burst & 0x7f);
  delayMicroseconds(100);
  Serial.print("FRAME:");
  for (int i = 0; i < 900; i++) {
    byte pixelValue = SPI.transfer(0);
    Serial.print(pixelValue);
    Serial.print(" ");
  }
  delayMicroseconds(15);
  adns_com_end();
  delayMicroseconds(5);
  Serial.println();

  //adns_write_reg(REG_Power_Up_Reset, 0x5A);
  //delay(50);
  //adns_write_reg(REG_LASER_CTRL0, 0x00);
}

void moveSteps(int steps, bool forward) {
  for (int s = 0; s < steps; s++) {
    for (int i = 0; i < 8; i++) {
      int index = forward ? i : (7 - i);
      setStep(halfStepSequence[index]);
      delay(delayTime);
    }
  }
}

void setStep(int step[4]) {
  digitalWrite(IN1, step[0]);
  digitalWrite(IN2, step[1]);
  digitalWrite(IN3, step[2]);
  digitalWrite(IN4, step[3]);
}

void moveHome() {
  if (digitalRead(PhotoUni) == LOW && startUni == false) {
    while (digitalRead(PhotoUni) == LOW) {
      moveSteps(10, false);
    }
    startUni = true;
  }
  if (digitalRead(PhotoBi) == LOW && startBi == false) {
    while (digitalRead(PhotoBi) == LOW) {
      analogWrite(3,150);
      myStepper.step(-20);
    }
    startBi = true;
  }
  analogWrite(3,0);
}
int row = 0;
int column = 0;

void loop() {
  moveHome();

  if (Serial.available() > 0) {
    String command = Serial.readStringUntil('\n');
    command.trim();

    if (command == "start") {
      shouldCapture = true;
      Serial.println("Adatküldés indítva...");
    } else if (command == "stop") {
      shouldCapture = false;
      Serial.println("Adatküldés leállítva...");
      row = 0;
      column = 0;
    } else if (command == "reset") {
      shouldCapture = false;
      row = 0;
      column = 0;
      startUni = false;
      startBi = false;
      moveHome();
      performStartup();
    }
  }
  if (shouldCapture) {
    sendFrame();
    column++;
    moveSteps(10, true);
    Serial.print("SS ");
    Serial.println(column);
    delay(100);
  }

  if (column == 30) {
    row++;
    if (row < 3) {//38
      column = 0;
      while (digitalRead(PhotoUni) == LOW) {
        moveSteps(10, false);
      }
      analogWrite(3,150);
      myStepper.step(20);
      analogWrite(3,0);
      Serial.println("NEW_ROW");
    } else {
      row = 0;
      column = 0;
      shouldCapture=false;
      Serial.println("END");
    }
  }
}
