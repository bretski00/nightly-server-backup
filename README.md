# nightly-server-backup

Setup environment variable `ADDITIONAL_ENTROPY` put in the values like 1 2 3 in a byte array
This is used for the decryption. Decryption uses current user.

## Executing Nightly Server Backup Exe
### Parameters
1. Network Path to backup storage ex `\\mybackup\backup\`
1. Encrypted file name and path ex `\myconfig.json`
1. Server backup items, this is the parameter for `wbadmin start backup -include:[backup items]`
    1. ex `D:\some-data-folder`

wbadmin is running with the `-vssCopy` flag to copy contents of a data folder\

## Build Notes
Build file into single exe
```
dotnet publish -p:PublishSingleFile=true --self-contained false
```