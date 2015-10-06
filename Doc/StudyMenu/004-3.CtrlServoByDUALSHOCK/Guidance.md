# 3. PS3コントローラを用いたサーボモータ制御
## 特別にいるもの
* サーボモータ(TowerPro社製SG90を推奨)
* PS3コントローラ(DUALSHOCK3 or SIXAXIS)
* USB-Bluetooth接続ドングル
* USBケーブル(A-miniBタイプ)

## 手順
### 1. ワイヤリング。
下図のようにGPIO端子と電子部品をブレッドボードに配線する。  
![Bread](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlServoMotor/RPi_CtrlServoMotor_bread.png?raw=true)  
![Circuit](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlServoMotor/RPi_CtrlServoMotor_circuit.png?raw=true)  

### 2. RPiとPS3コントローラを接続しておく。
手順は以下を参考にする。  
* [002-3.CtrlGPIOByDUALSHOCK/Guidance.md](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/StudyMenu/002-3.CtrlGPIOByDUALSHOCK/Guidance.md)

### 3. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git pull`` する。

### 4. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/servoDualShock](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/servoDualShock)  
``cd App/servoDualShock`` 

### 5. コンパイルして実行ファイルを作成する。  
``make``  

### 6. 管理者権限で実行ファイルを実行する。  
``sudo ./servoDualShock``  
PS3コントローラの左ジョイスティックの傾きに応じてサーボモータのホーンが回転すれば成功！！  
