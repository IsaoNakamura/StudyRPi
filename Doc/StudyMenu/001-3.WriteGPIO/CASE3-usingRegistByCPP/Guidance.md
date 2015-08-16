# CASE 3: // C++コードによるレジストリ操作でLEDチカチカ

1. RPi勉強会用GitHubリポジトリをクローン(チェックアウト)して、ディレクトリに侵入する。  
``git clone https://github.com/IsaoNakamura/StudyRPi/``  
``cd StudyRPi``  
  既にクローンしていれば、``git fetch`` もしくは、 ``git pull`` する。

2. 以下のAPPを使用するので、このAPPのディレクトリに侵入する。  
[StudyRPi/App/outputGPIO](https://github.com/IsaoNakamura/StudyRPi/blob/wrkFirstPush/App/outputGPIO)  
``cd App/outputGPIO`` 

3. コンパイルして実行ファイルを作成する。  
``make``  

6. 管理者権限で実行ファイルを実行する。  
``sudo ./outputGPIO``  
このとき、ワイヤリングしたLEDが点滅すれば成功！！
