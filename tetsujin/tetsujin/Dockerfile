FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim
ARG source
ENV BUILD_TIME="${BUILD_TIME}"
WORKDIR /app
RUN apt-get -y update &&\
    apt-get -y install libgdiplus &&\
    cd /usr/lib &&\
    ln -s libgdiplus.so gdiplus.dll
EXPOSE 80
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "tetsujin.dll"]
