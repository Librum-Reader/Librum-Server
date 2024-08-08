# Librum-Server
The Librum-Server includes the API, database, and other fundamental infrastructure required for the "backend" of all Librum client applications and the website.

The server is written in C# using ASP.NET Core. The codebase can be developed, built, run, and deployed cross-platform on Windows, macOS, and Linux.

<br>

# Self-hosting 
Librum-Server can easily be self-hosted. This way all your data and books remain on your own devices and are not synchronized to the official cloud.

## 🐋 With Docker
Librum-Server can be run with Docker. We provide a [docker-compose.yml](docker-compose.yml) file as well as our own images. We are using GitHub's `ghcr.io` Container Registry.

```bash
wget https://github.com/Librum-Reader/Librum-Server/raw/main/docker-compose.yml

docker compose up -d
```

## 📃 Manual installation
If you don't like Docker, you can also selfhost Librum-Server by running it as a service on your linux server. Instructions can be found [here](self-hosting/self-host-installation.md).

<br>

# Contributing
Feel free to reach out to us via email (contact@librumreader.com) or discord (m_david#0631) if you are interested in contributing.<br>
<br>
We are following a pull request workflow where every contribution is sent as a pull request and merged into the dev/develop branch for testing.
Please make sure to keep to the conventions used throughout the application and ensure that all tests pass, before submitting any pull request.
