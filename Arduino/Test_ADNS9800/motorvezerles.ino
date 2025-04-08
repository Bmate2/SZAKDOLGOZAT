// Motorvezérlő bemenetek
#define IN1 2
#define IN2 3
#define IN3 4
#define IN4 5
#define PhotoIn A0

bool startPosition=false;

int halfStepSequence[8][4] = {
  {1, 0, 0, 0},
  {1, 1, 0, 0},
  {0, 1, 0, 0},
  {0, 1, 1, 0},
  {0, 0, 1, 0},
  {0, 0, 1, 1},
  {0, 0, 0, 1},
  {1, 0, 0, 1}
};

int delayTime = 5; 

void setup() {
  Serial.begin(9600);
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT);
  pinMode(IN4, OUTPUT);
  pinMode(PhotoIn, INPUT);

  
  //moveSteps(2048, false); // vissza
}

void loop() {
  if (digitalRead(PhotoIn)==LOW && startPosition==false){
    while(digitalRead(PhotoIn)==LOW){
      moveSteps(10, true);
    }
    startPosition=true;
    Serial.println("Kezdő pozíció!");
  }
  
}

// Half-step mozgás irány és lépésszám alapján
void moveSteps(int steps, bool forward) {
  for (int s = 0; s < steps; s++) {
    for (int i = 0; i < 8; i++) {
      int index = forward ? i : (7 - i);
      setStep(halfStepSequence[index]);
      delay(delayTime);
    }
  }
}

// Lépés beállítása
void setStep(int step[4]) {
  digitalWrite(IN1, step[0]);
  digitalWrite(IN2, step[1]);
  digitalWrite(IN3, step[2]);
  digitalWrite(IN4, step[3]);
}
/*
// Motorvezérlő bemenetek
#define IN1 2
#define IN2 3
#define IN3 4
#define IN4 5
#define PhotoUni A0
#define PhotoBi A1
#define BI1 6
#define BI2 7
#define BI3 8
#define BI4 9

bool startUni=false;
bool startBi=false;
int halfStepSequence[8][4] = {
  {1, 0, 0, 0},
  {1, 1, 0, 0},
  {0, 1, 0, 0},
  {0, 1, 1, 0},
  {0, 0, 1, 0},
  {0, 0, 1, 1},
  {0, 0, 0, 1},
  {1, 0, 0, 1}
};

int delayTime = 5; 

void setup() {
  Serial.begin(9600);
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT);
  pinMode(IN4, OUTPUT);
  
  pinMode(PhotoUni, INPUT);
  pinMode(PhotoBi, INPUT);

  pinMode(BI1,OUTPUT);
  pinMode(BI2,OUTPUT);
  pinMode(BI3,OUTPUT);
  pinMode(BI4,OUTPUT);

  
  //moveSteps(2048, false); // vissza
}

void loop() {
  if (digitalRead(PhotoUni)==LOW && startUni==false){
    while(digitalRead(PhotoUni)==LOW){
      moveSteps(10, true, false);
    }
    startUni=true;
    Serial.println("Kezdő pozíció vizszintesen!");
  }
  if(digitalRead(PhotoBi)==LOW && startBi==false){
    while(digitalRead(PhotoBi)==LOW){
      moveSteps(10, true, true);
    }
    startBi=true;
    Serial.println("Kezdő pozíció függőleges!");
  }
  
}

void moveSteps(int steps, bool forward, bool isBipolar) {

  if(isBipolar){
    for (int s = 0; s < steps; s++) {
      for (int i = 0; i < 8; i++) {
        int index = forward ? i : (7 - i);
        setStep(halfStepSequence[index],BI1,BI2,BI3,BI4);
        delay(delayTime);
      }
    }
  }
  else{
    for (int s = 0; s < steps; s++) {
      for (int i = 0; i < 8; i++) {
        int index = forward ? i : (7 - i);
        setStep(halfStepSequence[index],IN1,IN2,IN3,IN4);
        delay(delayTime);
      }
    }
  }
  
}

// Lépés beállítása
void setStep(int step[4], int in1, int in2, int in3, int in4) {
  digitalWrite(in1, step[0]);
  digitalWrite(in2, step[1]);
  digitalWrite(in3, step[2]);
  digitalWrite(in4, step[3]);
}
*/
