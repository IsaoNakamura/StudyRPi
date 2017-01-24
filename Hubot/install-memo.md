## 0. RaspberryPiにインストールされているパッケージのアップデート・アップグレード
### 
``sudo apt-get update``  
``sudo apt-get upgrade``

## 1. node.js(v0.12)のインストール
``curl -sL https://deb.nodesource.com/setup_0.12 | sudo bash -``  
``sudo apt-get install nodejs``

## 2. npmのインストールとアップデート
``sudo npm install -g npm``  
``sudo npm install -g n``

## 3. hubot関係モジュールをインストール
```sudo npm install -g yo generator-hubot coffee-script```

## 4.botの作成
```
mkdir mybot
cd mybot
yo hubot
```
