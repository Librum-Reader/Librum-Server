# Librum-Server
The Librum-Server contains the API, database, and other core infrastructure items needed for the "backend" of all Librum client applications.

The server is written in C# using .NET Core with ASP.NET Core. The codebase can be developed, built, run, and deployed cross-platform on Windows, macOS, and Linux distributions.

<br>

# Getting started

Instructions to get Librum-Server up and running in your environment.

### Note
The Librum-Server RestAPI is preconfigured with Librum's servers, which for obvious reasons, is not accessible to everyone. If you plan to run your own instance of Librum-Server, you will need to change the connection strings (For the SQL-Server and the Blob-Storage provider) to point to your own servers. You can do this by adding your connection strings to the appsettings file (https://github.com/Librum-Reader/Librum-Server/blob/main/src/Presentation/appsettings.Development.json).

### Prerequisites
- .NET 7 (https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

### Installation
1. Clone the repository.
    ```sh
    git clone -j4 https://github.com/Librum-Reader/Librum-Server.git
    ```
2. Step into the cloned project folder.
    ```sh
    cd Librum-Server/
    ```
3. Restore the dependencies.
    ```sh
    dotnet restore .
    ```
4. Build the application.
    ```sh
    dotnet build .
    ```
5. Run
    ```sh
    cd src/Presentation
    dotnet run
    ```
    
    
# Contributing
Feel free to reach out to us via email (contact@librumreader.com) or discord (m_david#0631) if you are interested in contributing.<br>
<br>
We are following a pull request workflow where every contribution is sent as a pull request and merged into the dev/develop branch for testing.
Please make sure to keep to the conventions used throughout the application and ensure that all tests pass, before submitting any pull request. 
