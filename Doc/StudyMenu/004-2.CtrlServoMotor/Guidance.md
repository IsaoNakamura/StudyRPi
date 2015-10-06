# 2. サーボモータ制御
## 特別にいるもの
* サーボモータ(TowerPro社製SG90を推奨)

## 手順
### 1. ワイヤリング。
下図のようにGPIO端子と電子部品をブレッドボードに配線する。  
![Bread](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlServoMotor/RPi_CtrlServoMotor_bread.png?raw=true)  
![Circuit](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlServoMotor/RPi_CtrlServoMotor_circuit.png?raw=true)  

### 2. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git pull`` する。

### 3. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/servoWiringPi](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/servoWiringPi)  
``cd App/servoWiringPi`` 

### 4. コンパイルして実行ファイルを作成する。  
``make``  

### 5. 管理者権限で実行ファイルを実行する。  
``sudo ./servoWiringPi``  
正の整数を入力してサーボモータがその値に対応する角度に設定されれば成功！！  
『-1』を入力してサーボモータが最小・最大の値に対応する角度に順次設定されプログラムが終了すれば成功!！！  