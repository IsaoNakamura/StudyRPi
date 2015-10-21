# 2. OpenCVインストール
## 特別にいるもの
* インターネット環境

## 手順
### 1. OpenCVを使うためのパッケージをインストールする。
1. 以下のコマンドでRPiの参照するパッケージリストを更新する。  
``sudo apt-get update``
2. 以下のコマンドでRPiにOpenCVを使うためのパッケージをインストールする。  
``sudo apt-get -y install build-essential cmake cmake-qt-gui pkg-config libpng12-0 libpng12-dev libpng++-dev libpng3 libpnglite-dev zlib1g-dbg zlib1g zlib1g-dev pngtools libtiff4-dev libtiff4 libtiffxx0c2 libtiff-tools libjpeg8 libjpeg8-dev libjpeg8-dbg libjpeg-progs ffmpeg libavcodec-dev libavcodec53 libavformat53 libavformat-dev libgstreamer0.10-0-dbg libgstreamer0.10-0 libgstreamer0.10-dev libxine1-ffmpeg libxine-dev libxine1-bin libunicap2 libunicap2-dev libdc1394-22-dev libdc1394-22 libdc1394-utils swig libv4l-0 libv4l-dev python-numpy libpython2.6 python-dev python2.6-dev libgtk2.0-dev pkg-config libswscale-dev``  
  
3. 以下のコマンドでOpenCVをダウンロードしてコンパイル・インストールする。  
``wget http://sourceforge.net/projects/ opencv-2.4.10.zip``  
``unzip opencv-2.4.10.zip``  
``cd opencv-2.4.10``  
``mkdir build``  
``cd build``  
``cmake -D CMAKE_BUILD_TYPE=RELEASE -D CMAKE_INSTALL_PREFIX=/usr/local -D BUILD_NEW_PYTHON_SUPPORT=ON -D BUILD_EXAMPLES=ON ..``  
``make -j 4``  
``sudo  make install``  
``sudo  ldconfig``  

