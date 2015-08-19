# 1. DCモーター制御
## 特別にいるもの
* DCモーター
* DC電源(例:電池+電池ボックス)
* パイポーラトランジスタ(NPN型)

## 手順
### 1. ワイヤリング。
下図のようにGPIO端子と電子部品をブレッドボードに配線する。  
![Bread](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlDCMotor/RPi_CtrlDCMotor_bread.png?raw=true)  
![Circuit](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlDCMotor/RPi_CtrlDCMotor_circuit.png?raw=true)  

### 2. RPiとPS3コントローラを接続しておく。
手順は以下を参考にする。  
* [002-3.CtrlGPIOByDUALSHOCK/Guidance.md](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/StudyMenu/002-3.CtrlGPIOByDUALSHOCK/Guidance.md)

### 3. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git fetch`` もしくは、 ``git pull`` する。

### 4. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/dualshockGPIO](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/dualshockGPIO)  
``cd App/dualshockGPIO`` 

### 5. コンパイルして実行ファイルを作成する。  
``make``  

### 6. 管理者権限で実行ファイルを実行する。  
``sudo ./dualshockGPIO``  
PS3コントローラの左ボタンを押したとき、DCモーターが回転すれば成功！！  
PS3コントローラの右ボタンを押したとき、DCモーターが止まれば成功！！  