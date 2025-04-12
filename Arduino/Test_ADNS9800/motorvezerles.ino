#include <Stepper.h>
#define IN1 2
#define IN2 3
#define IN3 4
#define IN4 5
#define PhotoUni A0
#define PhotoBi A1

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

const int stepsPerRevolution = 200;
Stepper myStepper(stepsPerRevolution, 9,8,7,6);
int stepBi=0;
void setup() {
  Serial.begin(9600);
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT);
  pinMode(IN4, OUTPUT);
  pinMode(PhotoUni, INPUT);
  pinMode(PhotoBi, INPUT);
  myStepper.setSpeed(20);
  Serial.begin(9600);
}

void loop() {

  delay(2000);

  if (digitalRead(PhotoUni)==LOW && startUni==false){
    while(digitalRead(PhotoUni)==LOW){
      moveSteps(10, false);
    }
    startUni=true;
  }
  if (digitalRead(PhotoBi)==LOW && startBi==false){
    while(digitalRead(PhotoBi)==LOW){
      myStepper.step(-20);
    }
    startBi=true;  
  }
  if(startUni && startBi){
    Serial.println("Kezdő pozíció!");
  }

  //moveSteps(300,true); //unipoláris max lépés, kaputól el true
  //myStepper.step(740); //bipoláris max, kaputól el plusz
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


