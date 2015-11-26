# 4. OpenCVとサーボモータを用いた顔追従
## 特別にいるもの
* UVC対応USBカメラ(動作確認済カメラ：Logicool C270)
* SG90用2軸アングルFPVカメラマウント
* サーボモータ×2(TowerPro社製SG90を推奨)

## 手順
### 1. ワイヤリング。
USBカメラをRaspberryPiに接続する。  
下図のようにGPIO端子と電子部品を配線する。  
![Bread](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlDualServo/RPi_CtrlDualServo_bread.png?raw=true)  
![Circuit](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlDualServo/RPi_CtrlDualServo_circuit.png?raw=true)  

### 2. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git pull`` する。

### 5. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/homingFaceSimple](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/homingFaceSimple)  
``cd App/homingFaceSimple``  

### 6. コンパイルして実行ファイルを作成する。  
``make``  

### 7. 管理者権限で実行ファイルを実行する。  
``sudo ./homingFaceSimple``  

カメラ映像のウィンドウが表示されて、顔が映ったら顔を画面の中央入れるようにサーボが動作したら成功！！  