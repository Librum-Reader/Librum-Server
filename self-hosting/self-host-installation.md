# Librum-Server
The build and deploy process was tested on Ubuntu 22.04. It should work on any other linux distribution, but the commands might need to be adjusted. 

<br>

## Dependencies

You will need `dotnet`, `openssl` and `mariadb-server`.

First enable the Microsoft PPA to be able to download dotnet
```
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb 
sudo dpkg -i packages-microsoft-prod.deb
```
then run
```
sudo apt install dotnet-sdk-7.0 openssl mariadb-server
```
to install all dependencies.

<br>

## Build

To build the server, clone the repository and use `dotnet publish`

```
git clone https://github.com/Librum-Reader/Librum-Server.git
cd Librum-Server
dotnet restore
cd src/Presentation
dotnet publish  -c Release -o build  --no-restore --verbosity m

```

<br>

## Install
### Create a `librum-server` user

```
sudo useradd -r -g librum-server -d /var/lib/librum-server --shell /usr/sbin/nologin librum-server 
```

### Install the .service file for systemd  

```
cd ../..
sudo install -d  /etc/systemd/system/
sudo install self-hosting/librum-server.service -m660  /etc/systemd/system/
```

### Install the .conf file that contains the environment variables

```
sudo install -d  /etc/librum-server/
sudo install -m660 self-hosting/librum-server.conf /etc/librum-server/
```

### Install the server

```
sudo mkdir -p /var/lib/librum-server/srv
sudo cp src/Presentation/build/* /var/lib/librum-server/srv  --recursive
sudo chmod --recursive 660 /var/lib/librum-server/
sudo chmod 770 /var/lib/librum-server
sudo chmod 770 /var/lib/librum-server/srv
sudo install self-hosting/run.sh -m770 /var/lib/librum-server/srv
sudo chown --recursive librum-server /var/lib/librum-server/
```

### Install the manpage  

```
mkdir -p /usr/share/man/man7  
sudo install -m664 self-hosting/librum-server.7 /usr/share/man/man7
```

### Insall readme

```
sudo install -m664 self-hosting/self-host-installation.md /var/lib/librum-server/srv
```

### Create the SSL certificate for the server  

```
KEYOUT=/var/lib/librum-server/srv/librum-server.key
CRTOUT=/var/lib/librum-server/srv/librum-server.crt
PFXOUT=/var/lib/librum-server/srv/librum-server.pfx
sudo /usr/bin/openssl req -x509 -newkey rsa:4096 -sha256 -days 365 -nodes -keyout $KEYOUT -out  $CRTOUT -subj "/CN=librum-server" -extensions v3_ca -extensions v3_req 
sudo openssl pkcs12 -export -passout pass: -out $PFXOUT -inkey $KEYOUT -in $CRTOUT
sudo chown librum-server $PFXOUT 
```

### Configure the server ports  

Edit `/var/lib/librum-server/srv/appsettings.json` and change it to look like the following:

```
{
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
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureKeyVaultUri": "https://librum-keyvault.vault.azure.net/",
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "ClientIdHeader": "X-ClientId",
    "HttpStatusCode": 429,
    "GeneralRules": [
      {
        "Endpoint": "post:/api/register",
        "Period": "15m",
        "Limit": 6
      }
    ]
  }
}
```

<br>

## Run

### Install and configure MariaDB

```
sudo apt install mariadb-server
```

Edit `/etc/mysql/mariadb.conf.d/50-server.cnf` (called differently on other linux distros e.g. `/etc/my.cnf.d/server.cnf` or `my.cnf`).

Set `bind-adress` to `127.0.0.1` and if a `skip-networking` section exists, comment it out by adding a `#` infront of it.

Then restart the mariaDB service:  

```
systemctl restart mysqld
```

#### Create Mysql user and password
For example:

```
sudo mysql_secure_installation

Switch to unix_socket authentication [Y/n] n
Change the root password? [Y/n] y
Remove anonymous users? [Y/n] y
Disallow root login remotely? [Y/n] y
Remove test database and access to it? [Y/n] y
Reload privilege tables now? [Y/n] y
```

### Run the librum-server
Firstly you must edit `/etc/librum-server/librum-server.conf` and change the variables following the comments above them.

Then you can run:

```
sudo systemctl daemon-reload
sudo systemctl start librum-server
```

to start the service.

<br>

## Note
- By default the server listens to 5000 (http) and (5001) https. You can chage it in the `/var/lib/librum-server/srv/appsettings.json` file.
- The server stores its files at `/var/librum-server/data_storage`
- Logs are written to `/var/librum-server/srv/Data`

<br>

## Configuration for the client application

By default the Librum client application is set up to use the official servers. To connect it with your self-hosted server, you will need to edit `~/.config/Librum-Reader/Librum.conf` and set `selfHosted=true` and `serverHost` to your server's url (e.g. `serverHost=https://127.0.0.1:5001`).<br>
If there is no file at `~/.config/Librum-Reader/Librum.conf`, make sure that you have ran the application at least once before for the settings files to be generated.
<br>
<br>
To switch back to the official servers, set `selfHosted=false` and `serverHost=api.librumreader.com`

<br>

## Questions

If you have any questions or run into problems which you can't solve, feel free to open an issue.
