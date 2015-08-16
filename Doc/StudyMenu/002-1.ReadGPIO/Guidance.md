# 1. GPIO端子を用いたスイッチ状態読み取り
## 特別にいるもの
* スイッチ

## 手順
### 1. ワイヤリング。
下図のようにGPIO端子と電子部品をブレッドボードに配線する。  
![Wiring](https://github.com/IsaoNakamura/StudyRPi/Doc/Wiring/RPi_ReadGPIO/RPi_ReadGPIO_bread.png?raw=true)

### 2. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git fetch`` もしくは、 ``git pull`` する。

### 3. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/readGPIO](https://github.com/IsaoNakamura/StudyRPi/App/readGPIO)  
``cd App/readGPIO`` 

### 4. コンパイルして実行ファイルを作成する。  
``make``  

### 5. 管理者権限で実行ファイルを実行する。  
``sudo ./readGPIO``  
ワイヤリングしたスイッチを押したとき、以下のようにGPIO_Bの値が1とコンソールに表示され、LEDが点灯すれば成功！！  
``XXX GPIO_B is 1``