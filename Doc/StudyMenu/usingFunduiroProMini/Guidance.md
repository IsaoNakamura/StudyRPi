# 1. funduiro pro miniをMacOSXでセットアップ
## 特別にいるもの
* [funduiro pro mini](http://ja.aliexpress.com/item/Free-Shipping-3pcs-lot-USB2-0-To-TTL-6Pin-CH340G-Converter-for-Arduino-PRO-Instead-of/1922500840.html?isOrigTitle=true) (arduiro pro mini互換機)
* [USB2 0 to TTL 6pin CH340G Converter x3](http://ja.aliexpress.com/item/Free-Shipping-new-version-5pcs-lot-Pro-Mini-328-Mini-ATMEGA328-5V-16MHz-for-Arduino/1656644616.html?adminSeq=220352482&shopNumber=1022067) (USBシリアルアダプタ) 

## 手順
### 1. funduiroにピンヘッダを半田付けする。
1. funduiroにピン数に合わせたピンヘッダをブレッドボードに差し込む。 
![Picture](https://github.com/IsaoNakamura/StudyRPi/blob/wrkCtrlArduino/Doc/StudyMenu/usingFunduiroProMini/00_setPin.JPG?raw=true) 
2. ピンヘッダをfunduiroに半田付けする。  
![Picture](https://github.com/IsaoNakamura/StudyRPi/blob/wrkCtrlArduino/Doc/StudyMenu/usingFunduiroProMini/01_setBoard.JPG?raw=true) 
3. 最後にUSBシリアルアダプタに接続するためのピンヘッダを半田付けする。  
![Picture](https://github.com/IsaoNakamura/StudyRPi/blob/wrkCtrlArduino/Doc/StudyMenu/usingFunduiroProMini/02_setConnecter.jpg?raw=true) 

### 2. MacにUSBシリアルアダプタのドライバをインストールする。
1. USBシリアルアダプタのドライバを以下のサイトからダウンロードする。  
http://www.homautomation.org/wp-content/uploads/2015/02/CH341SER_MAC-1.zip  
2. ダウンロードしたzipファイルを解凍してパッケージをインストールする。  
3. Macを再起動する。  

#### 2-1. MacOSXのバージョンが「El Capitan」である場合
1. 「command」キーと「R」キーを同時押しをしながらMacを再起動してリカバリーモードを立ち上げる。  
2. メニューのツール内からターミナルを起動して以下のコマンドを実行する。  
``csrutil enable --without kext`` 
3. Macを再起動する。  
``reboot`` 

#### 2-2. MacOSXのバージョンが「Yosemite」である場合
1. ターミナルを起動して以下のコマンドを実行する。  
``sudo nvram boot-args="kext-dev-mode=1"`` 
2. Macを再起動する。  

### 3. funduiroをUSBシリアルアダプタでMacにUSB接続する。

### 4. ArduinoIDEを起動してメニューのツール内のポートに以下があることを確認できれば成功。  
* /dev/cu.wchusbserial*