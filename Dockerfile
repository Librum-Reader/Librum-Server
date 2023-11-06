FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

COPY . .
RUN dotnet restore
WORKDIR ./src/Presentation
RUN dotnet publish -c Release -o build  --no-restore --verbosity m

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

RUN groupadd -r -f librum-server
RUN useradd -r -g librum-server -d /var/lib/librum-server --shell /usr/sbin/nologin librum-server


RUN mkdir -p /var/lib/librum-server/srv
COPY --from=build /app/src/Presentation/build/* /var/lib/librum-server/srv
COPY --from=build /app/appsettings.json /var/lib/librum-server/srv
RUN chmod --recursive 660 /var/lib/librum-server/
RUN chmod 770 /var/lib/librum-server
RUN chmod 770 /var/lib/librum-server/srv
COPY --from=build /app/self-hosting/run.sh .
RUN install run.sh -m770 /var/lib/librum-server/srv
RUN rm -f /app/run.sh
RUN chown --recursive librum-server /var/lib/librum-server/

ENV JWTValidIssuer=exampleIssuer
ENV JWTKey=exampleOfALongSecretToken
ENV SMTPEndpoint=smtp.example.com
ENV SMTPUsername=mailuser123
ENV SMTPPassword=smtpUserPassword123
ENV SMTPMailFrom=mailuser123@example.com
ENV AdminEmail=admin@example.com
ENV AdminPassword=strongPassword123
ENV DBConnectionString=Server=mariadb;port=3306;Database=librum;Uid=librum;Pwd=mariadb;
ENV CleanUrl=http://0.0.0.0
ENV OpenAIToken=
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_PRINT_TELEMETRY_MESSAGE=false
ENV LIBRUM_SELFHOSTED=true

EXPOSE 5000/tcp
EXPOSE 5001/tcp

WORKDIR /var/lib/librum-server/srv
USER librum-server

# TODO: Add VOLUME directives

ENTRYPOINT [ "/var/lib/librum-server/srv/run.sh" ]
