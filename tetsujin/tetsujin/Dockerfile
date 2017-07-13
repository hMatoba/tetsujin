FROM microsoft/aspnetcore:1.1.2
ARG source
WORKDIR /app

RUN apt-get -y update
RUN apt-get -y install libgdiplus && \
    cd /usr/lib && \
    ln -s libgdiplus.so gdiplus.dll

EXPOSE 5001
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "tetsujin.dll"]
