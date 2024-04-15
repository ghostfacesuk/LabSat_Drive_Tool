@echo off
sc create Ext2Srv binPath= "Ext2Srv.exe" start= auto
sc start Ext2Srv