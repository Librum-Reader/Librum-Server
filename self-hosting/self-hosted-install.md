# Librum-Server
Whole build and deploy process was tested on ALT Linux, feel free to ask questions.

The Librum-Server contains the API, database, and other core infrastructure items needed for the "backend" of all Librum client applications.


**You need to change server url in cliet app to use it.**

## Dependencies

1. To build server you need: dotnet-sdk-7.0 dotnet-7.0  dotnet-host openssl 
2. To run service you need: mariadb-server dotnet-7.0 dotnet-aspnetcore-runtime-7.0 openssl

So:  

```
apt-get install dotnet-sdk-7.0 dotnet-7.0  dotnet-host openssl mariadb-server  dotnet-aspnetcore-runtime-7.0 
```

## Build process

To build server you need access to web to download nuget packages at flight  

```
git clone https://github.com/Librum-Reader/Librum-Server.git librum-server 
cd librum-server
dotnet restore
cd src/Presentation
dotnet publish  -c Release -o build  --no-restore --verbosity m

```


### Build in isolated invironment without network  
To build without network you need to pre-download nuget packages. It can be made by runnig 

```dotnet restore --packages <PATH>```  

it will download all dependenies to local path. Then You can take all .nupkg files and restore project with  

```dotnet restore -s <PATH_TO_FOLDER_WITH_NUPKGS>```  

The rest of building processss is not different

## Installation
### Create user

```
/usr/sbin/groupadd -r -f librum-server
/usr/sbin/useradd -r -g librum-server -d /var/lib/librum-server -s /dev/null -c "Server for librum-reader"  -n %name >/dev/null 2>&1 ||:
```

### Install .service file for systemd  

```
install -d  /etc/systemd/system/
install self-hosting/librum-server.service -m660  /etc/systemd/system/
```

### Install .conf file for secret variables

```
install -d  /etc/librum-server/
install -m660 self-hosting/librum-server.conf /etc/librum-server/
```

### Install app  

```
mkdir -p /var/lib/librum-server/srv
cp src/Presentation/build/* /var/lib/librum-server/srv  --recursive
chmod --recursive 660 /var/lib/librum-server/
chmod 770 /var/lib/librum-server
chmod 770 /var/lib/librum-server/srv
install self-hosting/run.sh -m770 /var/lib/librum-server/srv
chown --recursive librum-server /var/lib/librum-server/
```
### Install manpage  

```
mkdir -p /usr/share/man/man7  
install -m664 self-hosting/librum-server.7 /usr/share/man/man7
```

### Insall readme

```
install -m664 self-hosting/self-hosted-install.md /var/lib/librum-server/srv
```

### Create SSL certificate  

```
KEYOUT=/var/lib/librum-server/srv/librum-server.key
CRTOUT=/var/lib/librum-server/librum-server.crt
PFXOUT=/var/lib/librum-server/srv/librum-server.pfx
/usr/bin/openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes -keyout $KEYOUT -out  $CRTOUT -subj "/CN=librum-server" -extensions v3_ca -extensions v3_req 
openssl pkcs12 -export -passout pass: -out $PFXOUT -inkey $KEYOUT -in $CRTOUT
chown librum-server $PFXOUT 
```
### Configure server ports  

go to src/Presentation/ and add to appsettings.json and appsettings.Development.json - this block:

```
  "Kestrel": {
    "EndPoints": {
      "Http": {
        "Url": "http://127.0.0.1:5000"
      },
      "Https": {
        "Url": "https://127.0.0.1:5001",
		  "Certificate": {
          "Path": "librum-server.pfx"
        }
      }
    }
  },
```




## Run

### Install and configure MariaDB

```
 apt-get mariadb-server
```
Go to /etc/my.cnf.d/server.cnf or my my.cnf  

```
vim  /etc/my.cnf.d/server.cnf 
```

You need to find:  

``` bind-adress== ```  

and make it look like  

```bind-adress=127.0.0.1```

Then you need to comment:  

```skip-networking```  

make it look like:  

```#skip-networking```  

Then restart mariaDB:  

```
systemctl restart mysqld
```

#### Create Mysql user and pass
Just for example:  

```
mysql -uroot
ALTER USER 'root'@'localhost' IDENTIFIED BY 'superStrongPass1';
```

### Run librum-server
First of all you must edit /etc/librum-reader/librum-reader.conf

```
vim /etc/librum-reader/librum-reader.conf
```
You must change at least ten variables, instructions are inside, feel free to ask if something is not clear.

Now you can run:

```
systemctl daemon-reload
systemctl start librum-service
```

## PS
By default, server listens to 5000(http) and (5001) https. You can chage it - look for json settings files in ```/var/librum-server/srv```  

Server uses local HD to store files in ```/var/librum-server/data_storage```  

Database structure is created with firs launch if not exists (dont forget to fill librum-reader.conf with db string)  

Logs are written to ```/var/librum-server/srv/Data```  




Whole build process was tested on ALT Linux, maybe you'll encounter some new difficulties on your way, I hope not.

