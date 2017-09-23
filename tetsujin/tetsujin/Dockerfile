FROM microsoft/aspnetcore:2.0
ARG source
WORKDIR /app
RUN apt -y update &&\
    apt -y install libgdiplus &&\
    cd /usr/lib &&\
    ln -s libgdiplus.so gdiplus.dll
EXPOSE 80
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "tetsujin.dll"]
