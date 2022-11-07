rem -- block port (first argument passed to batch script)
netsh advfirewall firewall add rule name="Ghost Recon: Future Soldier" dir=in action=block protocol=ANY remoteip=185.38.21.83,185.38.21.84,185.38.21.85
netsh advfirewall firewall add rule name="Ghost Recon: Future Soldier" dir=out action=block protocol=ANY remoteip=185.38.21.83,185.38.21.84,185.38.21.85
exit
