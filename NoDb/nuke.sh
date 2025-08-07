sudo rm -f /usr/lib{,64}/libGL.so.* /usr/lib{,64}/libEGL.so.*
sudo rm -f /usr/lib{,64}/xorg/modules/extensions/libglx.so
sudo dnf reinstall -y xorg-x11-server-Xorg mesa* libglvnd*
sudo mv /etc/X11/xorg.conf /etc/X11/xorg.conf.saved
