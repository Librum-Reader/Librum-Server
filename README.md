# Librum-Server
The Librum-Server includes the API, database, and other fundamental infrastructure required for the "backend" of all Librum client applications and the website.

The server is written in C# using ASP.NET Core. The codebase can be developed, built, run, and deployed cross-platform on Windows, macOS, and Linux.

<br>

# Self-hosting
Librum-Server can easily be self-hosted. This way all your data and books remain on your own devices and are not synchronized to the official cloud.<br>
You can find the instructions on how to self-host Librum-Server [here](self-hosting/self-host-installation.md).

## Docker
Before deploying the containers you may wish to update the login credentials defined as environment variables in `docker-compose.yml` and to configure a volume for both the database and Librum-server. Volumes are already included for convenience but disabled by default.

Navigate to the root of the project and run the following command to build and launch a Librum-server instance;
```shell
docker compose up -d
```
Once running you should have a Librum-server instance running and accessible on `127.0.0.1:5000`.

<br>

# Contributing
Feel free to reach out to us via email (contact@librumreader.com) or discord (m_david#0631) if you are interested in contributing.<br>
<br>
We are following a pull request workflow where every contribution is sent as a pull request and merged into the dev/develop branch for testing.
Please make sure to keep to the conventions used throughout the application and ensure that all tests pass, before submitting any pull request.
