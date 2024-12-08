#include <Arduino.h>


void setup() {
  Serial.begin(9600);
  // 3D mátrix (2x2x3 példa: 2 sor, 2 oszlop, 3 színkomponens: RGB)
  
}

void loop() {
 int matrix[2][2][3] = {
    {{255, 0, 0}, {0, 255, 0}},  // Sor 1: Piros, Zöld
    {{0, 0, 255}, {255, 255, 0}} // Sor 2: Kék, Sárga
  };
  delay(200);
  // Küldjük el a mátrix dimenzióit
  Serial.println("2,2,3");

  // Küldjük a mátrix értékeit soronként
  for (int i = 0; i < 2; i++) {
    for (int j = 0; j < 2; j++) {
      for (int k = 0; k < 3; k++) {
        Serial.print(matrix[i][j][k]);
        if (k < 2) Serial.print(","); // RGB komponensek vesszővel elválasztva
      }
      Serial.println(); // Új oszlop
    }
  }
}
