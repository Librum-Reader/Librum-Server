# Librum-Server
Whole build and deploy process was tested on  Ubuntu 22.04, feel free to ask questions.

The Librum-Server contains the API, database, and other core infrastructure items needed for the "backend" of all Librum client applications.


**You need to change server url in cliet app to use it.**

## Dependencies

1. To build server you need: dotnet-sdk-7.0 dotnet-7.0  dotnet-host openssl 
2. To run service you need: mariadb-server dotnet-7.0 dotnet-aspnetcore-runtime-7.0 openssl

So:  

```
sudo apt-get install dotnet-sdk-7.0 dotnet7 dotnet-host  aspnetcore-runtime-7.0  openssl mariadb-server

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
sudo /usr/sbin/useradd -r -g librum-server -d /var/lib/librum-server --shell /usr/sbin/nologin librum-server 
```

### Install .service file for systemd  

```
cd ../..
sudo install -d  /etc/systemd/system/
sudo install self-hosting/librum-server.service -m660  /etc/systemd/system/
```

### Install .conf file for secret variables

```
sudo install -d  /etc/librum-server/
sudo install -m660 self-hosting/librum-server.conf /etc/librum-server/
```

### Install app  

```
sudo mkdir -p /var/lib/librum-server/srv
sudo cp src/Presentation/build/* /var/lib/librum-server/srv  --recursive
sudo chmod --recursive 660 /var/lib/librum-server/
sudo chmod 770 /var/lib/librum-server
sudo chmod 770 /var/lib/librum-server/srv
sudo install self-hosting/run.sh -m770 /var/lib/librum-server/srv
sudo chown --recursive librum-server /var/lib/librum-server/
```
### Install manpage  

```
mkdir -p /usr/share/man/man7  
sudo install -m664 self-hosting/librum-server.7 /usr/share/man/man7
```

### Insall readme

```
sudo install -m664 self-hosting/ubuntu-self-hosted-install.md /var/lib/librum-server/srv
```

### Create SSL certificate  

```
KEYOUT=/var/lib/librum-server/srv/librum-server.key
CRTOUT=/var/lib/librum-server/srv/librum-server.crt
PFXOUT=/var/lib/librum-server/srv/librum-server.pfx
sudo /usr/bin/openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes -keyout $KEYOUT -out  $CRTOUT -subj "/CN=librum-server" -extensions v3_ca -extensions v3_req 
sudo openssl pkcs12 -export -passout pass: -out $PFXOUT -inkey $KEYOUT -in $CRTOUT
chown librum-server $PFXOUT 
```
### Configure server ports  

go to /var/lib/librum-server/srv/ and add to appsettings.json and appsettings.Development.json - this block:

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
sudo apt-get mariadb-server
```

Go to   /etc/mysql/mariadb.conf.d/  and find 50-server.cnf 

```
vim  /etc/mysql/mariadb.conf.d/50-server.cnf
```

You need to find:  

``` bind-adress== ```  

and make it look like  

```bind-adress=127.0.0.1```

Then you need to comment (if it exists):  

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
sudo mysql_secure_installation

Switch to unix_socket authentication [Y/n] n
Change the root password? [Y/n] y
Remove anonymous users? [Y/n] y
Disallow root login remotely? [Y/n] y
Remove test database and access to it? [Y/n] y
Reload privilege tables now? [Y/n] y
```

### Run librum-server
First of all you must edit /etc/librum-reader/librum-reader.conf

```
vim /etc/librum-reader/librum-reader.conf
```
You must change at least ten variables, instructions are inside, feel free to ask if something is not clear.

Now you can run:

```
sudo systemctl daemon-reload
sudo systemctl start librum-server
```

## PS
By default, server listens to 5000(http) and (5001) https. You can chage it - look for json settings files in ```/var/librum-server/srv```  

Server uses local HD to store files in ```/var/librum-server/data_storage```  

Database structure is created with firs launch if not exists (dont forget to fill librum-reader.conf with db string)  

Logs are written to ```/var/librum-server/srv/Data```  




Whole build process was tested on Ubuntu 22.04, maybe you'll encounter some new difficulties on your way, I hope not.

