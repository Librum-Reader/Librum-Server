# Librum-Server
The Librum-Server includes the API, database, and other fundamental infrastructure required for the "backend" of all Librum client applications and the website.

The server is written in C# using ASP.NET Core. The codebase can be developed, built, run, and deployed cross-platform on Windows, macOS, and Linux.

<br>

# Self-hosting

Librum-Server can easily be self-hosted. This way all your data and books remain on your own devices and are not synchronized to the official cloud.<br>
You can find the instructions on how to self-host Librum-Server [here](self-hosting/self-host-installation.md).


<br>

# Running with Docker

Librum-Server can ran with Docker. We currently do not provide images in DockerHub so you'll need to build it yourself.

```bash
git clone https://github.com/Librum-Reader/Librum-Server
cd Librum-Server
docker build . -t librum
```

Librum will also need a database. You can use [our sample docker-compose.yml file](docker-compose.yml) as a guide for setting it up.


<br>

# Contributing
Feel free to reach out to us via email (contact@librumreader.com) or discord (m_david#0631) if you are interested in contributing.<br>
<br>
We are following a pull request workflow where every contribution is sent as a pull request and merged into the dev/develop branch for testing.
Please make sure to keep to the conventions used throughout the application and ensure that all tests pass, before submitting any pull request.
