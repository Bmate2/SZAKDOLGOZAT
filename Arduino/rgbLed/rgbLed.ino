const int redPin=9;
const int greenPin=6;
const int bluePin=5;
const int ldrPin=A0;

const float redCorrection=0.7;
const float greenCorrection=0.6;
const float blueCorrection=1.0;
void setup() {
  pinMode(redPin,OUTPUT);
  pinMode(greenPin,OUTPUT);
  pinMode(bluePin,OUTPUT);
  Serial.begin(9600);

}

void loop() {
  delay(1000);
  SetColor(255,0,0);
  delay(500);
  int redIntensity=analogRead(ldrPin);
  Serial.print("Red intensity:");
  Serial.println(redIntensity);
  
  SetColor(0,255,0);
  delay(500);
  int greenIntensity=analogRead(ldrPin);
  Serial.print("Green intensity:");
  Serial.println(greenIntensity);
  
  SetColor(0,0,255);
  delay(500);
  int blueIntensity=analogRead(ldrPin);
  Serial.print("Blue intensity:");
  Serial.println(blueIntensity);

  SetColor(0,0,0);
  delay(2000);

}

void SetColor(int r, int g, int b){
  analogWrite(redPin,r*redCorrection);
  analogWrite(greenPin,g*greenCorrection);
  analogWrite(bluePin,b*blueCorrection);
}
