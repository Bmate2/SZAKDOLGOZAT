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

volatile byte movementflag=0;
volatile byte xydat[4];
int16_t * x = (int16_t *) &xydat[0];
int16_t * y = (int16_t *) &xydat[2];

volatile unsigned long lastInterruptTime = 0;

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
  increaseShutterTime();
  enableAutoExposure();
  delay(100);
  initComplete=9;
}

void enableAutoExposure() {
    digitalWrite(SS, LOW);
    SPI.transfer(0x0F); // Configuration_I regiszter
    SPI.transfer(0x01); // Auto-exposure engedélyezése
    digitalWrite(SS, HIGH);

    Serial.println("Automatikus expozíció engedélyezve.");
}

void increaseShutterTime() {
    digitalWrite(SS, LOW);
    SPI.transfer(0x0B); // Shutter_Lower regiszter
    SPI.transfer(0xFF); // Maximum érték
    digitalWrite(SS, HIGH);

    delayMicroseconds(50);

    digitalWrite(SS, LOW);
    SPI.transfer(0x0C); // Shutter_Upper regiszter
    SPI.transfer(0xFF); // Maximum érték
    digitalWrite(SS, HIGH);

    Serial.println("Expozíció maximálisra állítva.");
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
  Serial.print("Firmware betöltve");
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
  adns_write_reg(REG_Configuration_I, 0xa4); // 8200 cpi
  delay(10);

  //enable laser(bit 0 = 0b), in normal mode (bits 3,2,1 = 000b)
  // reading the actual value of the register is important because the real
  // default value is different from what is said in the datasheet, and if you
  // change the reserved bytes (like by writing 0x00...) it would not work.
  
  byte laser_ctrl0 = adns_read_reg(REG_LASER_CTRL0);
  laser_ctrl0 &= 0xf0;
  adns_write_reg(REG_LASER_CTRL0, laser_ctrl0 );
  delay(1);
}
void sendFrame() {
    // Serial.println("Capturing frame: "); 
  // send instructions to the adns to capture a frame. 
  adns_com_begin();
  delayMicroseconds(1);
  SPI.transfer(REG_Frame_Capture | 0x80); 
  SPI.transfer(0x93); 
  delayMicroseconds(120); 
  SPI.transfer(REG_Frame_Capture | 0x80); 
  SPI.transfer(0xc5); 
  delayMicroseconds(120);

  // wait two frames 
  adns_com_end(); 
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
  
    for (int row = 0; row < 30; row++) {
        Serial.print("Beérkező sor: ");
        int count = 0;
        
        for (int col = 0; col < 30; col++) {
            byte pixelValue = SPI.transfer(0);  // Pixel beolvasása
            Serial.print(pixelValue);
            Serial.print(",");
            count++;
        }
        Serial.println();

        if (count != 30) {  // Ha egy sor nem 30 hosszú, akkor hiba
            Serial.print("HIBA: Rossz hossz! ");
            Serial.println(count);
        }
        delayMicroseconds(15);
    }
 
  // end operation. 
  adns_com_end();
  //delayMicroseconds(100); // time to wait before another frame can be captured.  
  delayMicroseconds(5); 
  Serial.println("\n#");
}

int posX = 0, posY = 0; 

void readMotion(){
    adns_com_begin();
    
    byte motion = adns_read_reg(0x02);
    int deltaX_L = (char)adns_read_reg(0x03); // Delta_X_L
    int deltaY_L = (char)adns_read_reg(0x05); // Delta_Y_L
    int deltaX_H = (char)adns_read_reg(0x04); // Delta_X_H
    int deltaY_H = (char)adns_read_reg(0x06); // Delta_Y_H
    
    adns_com_end();  

    int16_t deltaX = (deltaX_H << 8) | (deltaX_L & 0xFF);
    int16_t deltaY = (deltaY_H << 8) | (deltaY_L & 0xFF);

    posX += deltaX;
    posY += deltaY;

    Serial.print("MOTION ");
    Serial.print(posX);
    Serial.print(",");
    Serial.println(posY);
  }


// int xdir = 0; 
// int ydir = 0; 

// void UpdatePointer(void){
//   byte motion = adns_read_reg(REG_Motion);
//   if(initComplete==9 && motion & 0x80){

//     xydat[0] = (byte)adns_read_reg(REG_Delta_X_L);
//     xydat[1] = (byte)adns_read_reg(REG_Delta_X_H); 
//     xydat[2] = (byte)adns_read_reg(REG_Delta_Y_L);
//     xydat[3] = (byte)adns_read_reg(REG_Delta_Y_H);
    
//     xdir = xdir + *x; 
//     ydir = ydir + *y; 
    
//     Serial.print("X:");
//     Serial.println((float)xdir / 200 * 25.4);
//     Serial.print("Y:");
//     Serial.println((float)ydir / 200 * 25.4);
    
    
//     movementflag=1;

//     }
//   }

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
    }
  }
  if(shouldCapture){
    readMotion();
    sendFrame();
    delay(100);
  }

  
}
