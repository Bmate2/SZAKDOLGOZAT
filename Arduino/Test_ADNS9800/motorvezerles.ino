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
