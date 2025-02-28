#include <SPI.h>
#include <avr/pgmspace.h>
#define REG_Product_ID                           0x00
#define REG_Revision_ID                          0x01
#define REG_Motion                               0x02
#define REG_Delta_X_L                            0x03
#define REG_Delta_X_H                            0x04
#define REG_Delta_Y_L                            0x05
#define REG_Delta_Y_H                            0x06
#define REG_SQUAL                                0x07
#define REG_Pixel_Sum                            0x08
#define REG_Maximum_Pixel                        0x09
#define REG_Minimum_Pixel                        0x0a
#define REG_Shutter_Lower                        0x0b
#define REG_Shutter_Upper                        0x0c
#define REG_Frame_Period_Lower                   0x0d
#define REG_Frame_Period_Upper                   0x0e
#define REG_Configuration_I                      0x0f
#define REG_Configuration_II                     0x10
#define REG_Frame_Capture                        0x12
#define REG_SROM_Enable                          0x13
#define REG_Run_Downshift                        0x14
#define REG_Rest1_Rate                           0x15
#define REG_Rest1_Downshift                      0x16
#define REG_Rest2_Rate                           0x17
#define REG_Rest2_Downshift                      0x18
#define REG_Rest3_Rate                           0x19
#define REG_Frame_Period_Max_Bound_Lower         0x1a
#define REG_Frame_Period_Max_Bound_Upper         0x1b
#define REG_Frame_Period_Min_Bound_Lower         0x1c
#define REG_Frame_Period_Min_Bound_Upper         0x1d
#define REG_Shutter_Max_Bound_Lower              0x1e
#define REG_Shutter_Max_Bound_Upper              0x1f
#define REG_LASER_CTRL0                          0x20
#define REG_Observation                          0x24
#define REG_Data_Out_Lower                       0x25
#define REG_Data_Out_Upper                       0x26
#define REG_SROM_ID                              0x2a
#define REG_Lift_Detection_Thr                   0x2e
#define REG_Configuration_V                      0x2f
#define REG_Configuration_IV                     0x39
#define REG_Power_Up_Reset                       0x3a
#define REG_Shutdown                             0x3b
#define REG_Inverse_Product_ID                   0x3f
#define REG_Motion_Burst                         0x50
#define REG_SROM_Load_Burst                      0x62
#define REG_Pixel_Burst                          0x64

byte initComplete=0;

const int ncs = 10;

extern const unsigned short firmware_length;
extern const unsigned char firmware_data[];

bool shouldCapture=false;

void setup() {
  Serial.begin(115200); 
  while (!Serial);
  
  pinMode(ncs, OUTPUT);
  pinMode(2, INPUT); 
  
  SPI.begin();
  SPI.setDataMode(SPI_MODE3);
  SPI.setBitOrder(MSBFIRST);
  SPI.setClockDivider(SPI_CLOCK_DIV8);
  
  //attachInterrupt(0, UpdatePointer, FALLING);

  performStartup();
  //enableAutoExposure();
  delay(100);
  initComplete=9;
}

void enableAutoExposure() {
    adns_write_reg(REG_Configuration_I, 0x01);
}

void adns_com_begin(){
  digitalWrite(ncs, LOW);
}

void adns_com_end(){
  digitalWrite(ncs, HIGH);
}

byte adns_read_reg(byte reg_addr){
  adns_com_begin();
  // send adress of the register, with MSBit = 0 to indicate it's a read
  SPI.transfer(reg_addr & 0x7f);
  delayMicroseconds(200); // tSRAD
  // read data
  byte data = SPI.transfer(0);
  
  delayMicroseconds(1); // tSCLK-NCS for read operation is 120ns
  adns_com_end();
  delayMicroseconds(19); //  tSRW/tSRR (=20us) minus tSCLK-NCS

  return data;
}

void adns_write_reg(byte reg_addr, byte data){
  adns_com_begin();
  
  //send adress of the register, with MSBit = 1 to indicate it's a write
  SPI.transfer(reg_addr | 0x80 );
  //sent data
  SPI.transfer(data);
  
  delayMicroseconds(20); // tSCLK-NCS for write operation
  adns_com_end();
  delayMicroseconds(100); // tSWW/tSWR (=120us) minus tSCLK-NCS. Could be shortened, but is looks like a safe lower bound 
}

void adns_upload_firmware(){
  // send the firmware to the chip, cf p.18 of the datasheet
  // set the configuration_IV register in 3k firmware mode
  
  adns_write_reg(REG_Configuration_IV, 0x02); // bit 1 = 1 for 3k mode, other bits are reserved 
  
  // write 0xd in SROM_enable reg for initializing
  adns_write_reg(REG_SROM_Enable, 0xd); 
  
  // wait for more than one frame period
  delay(10); // assume that the frame rate is as low as 100fps... even if it should never be that low
  
  // write 0x8 to SROM_enable to start SROM download
  adns_write_reg(REG_SROM_Enable, 0x8); 
  
  // write the SROM file (=firmware data) 
  adns_com_begin();
  SPI.transfer(REG_SROM_Load_Burst | 0x80); // write burst destination adress
  delayMicroseconds(15);
  
  // send all bytes of the firmware
  unsigned char c;
  for(int i = 0; i < firmware_length; i++){ 
    c = (unsigned char)pgm_read_byte(firmware_data + i);
    SPI.transfer(c);
    delayMicroseconds(15); 
  }
  adns_com_end();
}

void performStartup(void){
  adns_com_begin(); // ensure that the Serial port is reset
  adns_com_end(); // ensure that the Serial port is reset
  adns_write_reg(REG_Power_Up_Reset, 0x5a); // force reset
  delay(50); // wait for it to reboot
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
  adns_write_reg(REG_Configuration_I, 0x09); // 8200 cpi
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
  adns_write_reg(REG_LASER_CTRL0, laser_ctrl0 & 0xf0 );
  delay(1);
  Serial.println("Szenzor indul....");
}
void sendFrame() {
    // Serial.println("Capturing frame: "); 
  // send instructions to the adns to capture a frame. 
  adns_write_reg(REG_Frame_Capture,0x93);
  delayMicroseconds(120); 
  adns_write_reg(REG_Frame_Capture,0xc5);
  delayMicroseconds(120);
 

  // wait two frames 
  //adns_com_end(); 
  // lower this value to increase the frame rate of the incoming data.
  delay(1); // assuming a very slow frame rate ~200Hz
  adns_com_begin(); 

  delayMicroseconds(100); //TsRAD

  // wait for adns to signal that it is ready to transfer data. 
  byte readys = 0;
  while(readys == 0){
    SPI.transfer(REG_Motion & 0x7f);
    delayMicroseconds(100); // tSRAD
    readys = SPI.transfer(0); 
    readys = readys & 1;
    delayMicroseconds(20);
  }

    // prepare to read the pixel burst register continuously.
    SPI.transfer(REG_Pixel_Burst & 0x7f); 
    delayMicroseconds(100); // tSRAD
    Serial.print("FRAME:");
    for (int i = 0;i<900;i++){
      byte pixelValue = SPI.transfer(0);  // Pixel beolvasása
      Serial.print(pixelValue);
      Serial.print(" ");
    }

  delayMicroseconds(15);
  // end operation. 
  adns_com_end();
  //delayMicroseconds(100); // time to wait before another frame can be captured.  
  delayMicroseconds(5); 
  Serial.println();

  adns_write_reg(REG_Power_Up_Reset, 0x5A);
  delay(50);
  adns_write_reg(REG_LASER_CTRL0, 0x00);

}

int row=0;
int column=0;

void loop() {
  if(Serial.available()>0){
    String command=Serial.readStringUntil('\n');
    command.trim();

    if(command=="start"){
      shouldCapture=true;
      Serial.println("Adatküldés indítva...");
    }
    else if(command=="stop"){
      shouldCapture=false;
      Serial.println("Adatküldés leállítva...");
      row=0;
      column=0;
    }
    else if(command=="reset"){
      shouldCapture=false;
      row=0;
      column=0;
      performStartup();
    }
  }
  if(shouldCapture){
    sendFrame();
    column++;
    delay(100);
  }

  if(column==20){
    if(row++<2){
      row++;
      column=0;
      Serial.println("NEW_ROW");
    }
    else{
      row=0;
      column=0;
      Serial.println("END");
    }
  }

}
