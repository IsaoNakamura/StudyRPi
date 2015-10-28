# 1. サーボモータ2軸制御
## 特別にいるもの
* SG90用2軸アングルFPVカメラマウント
* サーボモータ×2(TowerPro社製SG90を推奨)
* PS3コントローラ(DUALSHOCK3 or SIXAXIS)
* USB-Bluetooth接続ドングル
* USBケーブル(A-miniBタイプ)

## 手順
### 1. FPVカメラマウントの組み立て
FPVカメラマウントには説明書などは入っていない。(少なくとも以下で購入すると無い)  
[Amazon.co.jp SG90サーボ用 2軸 カメラマウント 2軸アングル FPV 空撮にも (マウント+SG90(2個))](http://www.amazon.co.jp/SG90%E3%82%B5%E3%83%BC%E3%83%9C%E7%94%A8-%E3%82%AB%E3%83%A1%E3%83%A9%E3%83%9E%E3%82%A6%E3%83%B3%E3%83%88-2%E8%BB%B8%E3%82%A2%E3%83%B3%E3%82%B0%E3%83%AB-%E7%A9%BA%E6%92%AE%E3%81%AB%E3%82%82-%E3%83%9E%E3%82%A6%E3%83%B3%E3%83%88/dp/B010BWNZO4/ref=sr_1_2?ie=UTF8&qid=1446012033&sr=8-2&keywords=FPV+SG90)  

なので、写真を見ながらサーボとカメラマウントを組み立てる。  
サーボホーンの形状がカメラマウントに合わないのでカッターなので削る必要がある。  
![Picture](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/StudyMenu/005-1.CtrlDualServo/FPV-CameraMount.png?raw=true)


### 2. ワイヤリング。
下図のようにGPIO端子と電子部品を配線する。  
![Bread](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlDualServo/RPi_CtrlDualServo_bread.png?raw=true)  
![Circuit](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_CtrlDualServo/RPi_CtrlDualServo?raw=true)  

### 3. RPiとPS3コントローラを接続しておく。
手順は以下を参考にする。  
* [002-3.CtrlGPIOByDUALSHOCK/Guidance.md](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/StudyMenu/002-3.CtrlGPIOByDUALSHOCK/Guidance.md)

### 4. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git pull`` する。

### 5. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/sampleServoDrvDual](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/sampleServoDrvDual)  
``cd App/sampleServoDrvDual`` 

### 6. コンパイルして実行ファイルを作成する。  
``make``  

### 7. 管理者権限で実行ファイルを実行する。  
``sudo ./sampleServoDrvDual``  
以下の動画のようにPS3コントローラの左ジョイスティックを上下左右に動かすとカメラマウントの上下左右に動けば成功！！  
[![IMAGE ALT TEXT HERE](http://img.youtube.com/vi/TNoqCQz9aLs/0.jpg)](http://www.youtube.com/watch?v=TNoqCQz9aLs)
