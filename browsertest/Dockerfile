FROM ubuntu

RUN apt-get update && apt-get upgrade -y
RUN apt-get install -y python3-pip
RUN apt-get install -y phantomjs

RUN pip3 install selenium

ENV QT_QPA_PLATFORM=offscreen

COPY . /usr/
WORKDIR /usr/

ENTRYPOINT python3 setup.py test