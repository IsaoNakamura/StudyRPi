# 2. SSHによるネットワーク経由操作
## いるもの
* PC(Windows or MacOSX)
* USBキーボード、USBマウス
* モニタ、HDMIケーブル
* USBmicroBケーブル、USB電源
* LANケーブル

## 手順  
例としてRPiのIPアドレスを``192.168.1.100``に固定する方法を明記する。  
### 1. RPiを固定IPアドレス化  
1. RPiにモニタとUSBキーボードをつなげて起動し、ログインしておく。  
2. ~~今から使うコマンドを管理者権限で実行可能にする。~~  
``sudo su``  
3. 接続するPCの有線LANポートのアドレスをメモしておく。  
(RPiを同じネットワークグループにするため)  
4. RPiのネットワーク設定ファイルをnanoエディタで開く。  
(今回はRPiに標準で入っているnanoというエディタを使用)  
``sudo nano /etc/network/interfaces``  

5. イーサネットインターフェース「eth0」のネットワーク設定を書き換える。  
``# iface eth0 inet dhcp``  
``iface eth0 inet static``  
``address 192.168.1.100``  

6. ネットワーク設定ファイルを保存する。  
保存するには``「ctrl」+「o」``を入力して``「Enter」``をターンッ！  
``「ctrl」+「x」``でエディタを終了する。  

7. イーサネットインターフェースを再起動する。  
``sudo ifdown eth0``  
``sudo ifup eth0  ``  
不安ならRPiごと再起動  
``sudo reboot``

8. ログインできる状態になったら、コンソールの少し上に設定したIPアドレスが表示されたことを確認する。  

### 2. PCからRPiにSSHでログインする。  
1. PCとRPiをLANケーブルで接続する。  
2. コンソールアプリでRPiにログインする。  
(Windowsなら「teraterm」とか、MacOSXなら「terminal」等。以下の例はMacOSX)  
``ssh pi@192.168.1.100``
