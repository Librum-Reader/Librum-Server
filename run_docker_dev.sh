# This script builds the Dockerfile and spawns a bash shell with the image
# for development purposes.
docker build . --tag librum
docker run -it --rm -v $(pwd):/librum --entrypoint "bash" -p 8080:8080 librum
