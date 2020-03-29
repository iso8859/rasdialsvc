# rasdialsvc
RasDial service

This windows service allow to autodial a vpn

1) Create VPN manualy and chek it can dial without asking for authentication
2) Edit rasdialsvc.json to set the ip adresse to ping to check if vpn is ok, set the vpn entry name, ping period, and wait after a disconnect and a connect.
3) From admin prompt
rasdialsvc install
During installation user+password is promped. Enter ./user + password. This is the user where ras entry is attached to.
rasdialsvc uninstall
rasdialsvc start
rasdialsvc stop

You are done
