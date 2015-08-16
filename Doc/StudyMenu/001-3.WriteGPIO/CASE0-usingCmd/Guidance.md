# CASE 0: // コマンド実行でLEDチカチカ

例としてGPIO17を使用する方法を明記する。  
コンソールアプリで以下のコマンドを順次実行する。

1. 今から使うコマンドを管理者権限で実行可能にする。  
``sudo su``

2. GPIO17の使用開始状態にする。  
``echo "17" > /sys/class/gpio/export``

3. GPIO17を出力として設定する。  
``echo "out" > /sys/class/gpio/gpio17/direction``

4. GPIO17をON(Highレベル)にする。  
``echo "1" > /sys/class/gpio/gpio17/value``  
このとき、ワイヤリングしたLEDが発光すれば成功！！

5. GPIO17をOFF(Lowレベル)にする。  
``echo "0" > /sys/class/gpio/gpio17/value``  
このとき、ワイヤリングしたLEDが消光すれば成功！！

6. GPIO17の使用終了状態にする。  
``echo "17" > /sys/class/gpio/unexport``

7. 管理者権限を終了する。  
``exit``
