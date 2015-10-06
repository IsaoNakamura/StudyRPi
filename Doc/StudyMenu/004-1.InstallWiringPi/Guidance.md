# 1. WiringPiインストール
## 特別にいるもの
* インターネット環境

## 手順
### 1. WiringPiのダウンロードとインストール
以下のサイトの手順に従う。  
http://wiringpi.com/download-and-install/

### 2. ワイヤリング。
下図のようにGPIO端子と電子部品をブレッドボードに配線する。  
![Bread](https://github.com/IsaoNakamura/StudyRPi/blob/master/Doc/Wiring/RPi_ReadGPIO/RPi_ReadGPIO_bread.png?raw=true)

### 3. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git pull`` する。

### 4. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/sampleWiringPi](https://github.com/IsaoNakamura/StudyRPi/blob/master/App/sampleWiringPi)  
``cd App/sampleWiringPi`` 

### 5. コンパイルして実行ファイルを作成する。  
``make``  

### 6. 管理者権限で実行ファイルを実行する。  
``sudo ./sampleWiringPi``  
ワイヤリングしたスイッチを押したとき、以下のようにGPIO_Bの値が1とコンソールに表示され、LEDが点灯すれば成功！！  
``XXX GPIO_B is 1``