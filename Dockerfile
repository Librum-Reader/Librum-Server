# syntax=docker/dockerfile:1
FROM alpine
WORKDIR /code
RUN apk add dotnet7-sdk
COPY . .
RUN dotnet restore .
RUN dotnet build .
WORKDIR src/Presentation
CMD dotnet run