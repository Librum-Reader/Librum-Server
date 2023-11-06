FROM debian:bookworm

WORKDIR /

# Prep dependencies
RUN apt-get update && \
	apt-get install -y openssl wget libicu-dev

# Not the prettiest .NET installation, but it will do
RUN wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh && \
	chmod +x ./dotnet-install.sh && \
	./dotnet-install.sh --channel 7.0 --install-dir /usr/local/bin && \
	rm -v ./dotnet-install.sh

# Build Librum
RUN mkdir -v /librum
COPY . /librum
WORKDIR /librum
RUN dotnet restore && \
	cd src/Presentation && \
	dotnet publish -c Release -o build --no-restore --verbosity m

# Install Librum
RUN groupadd -r -f librum-server && \
	useradd -r -g librum-server -d /var/lib/librum-server -m --shell /usr/sbin/nologin librum-server && \
	mkdir -pv /var/lib/librum-server/srv && \
	cp -r src/Presentation/build/* /var/lib/librum-server/srv/ && \
	chmod -R 660 /var/lib/librum-server/srv/* && \
	install ./self-hosting/run.sh -m770 /var/lib/librum-server/srv

# Copy appsettings to image
COPY ./docker/appsettings.json /var/lib/librum-server/srv/appsettings.json

# Ensure librum-server userhome ownership is set properly
RUN chown -R librum-server:librum-server /var/lib/librum-server/

# Cleanup
RUN rm -rf /librum && \
	apt-get clean autoclean

USER librum-server
WORKDIR /var/lib/librum-server/srv
ENTRYPOINT /var/lib/librum-server/srv/run.sh
