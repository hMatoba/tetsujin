# !/bin/sh
if test "$TRAVIS_BRANCH" = "master"; then
  docker tag matoba/tetsujin:ci matoba/tetsujin:latest
  docker login -u="${DOCKER_USERNAME}" -p="${DOCKER_PASSWORD}"
  docker push matoba/tetsujin:latest
  wget https://hyper-install.s3.amazonaws.com/hyper-linux-x86_64.tar.gz
  tar xzf hyper-linux-x86_64.tar.gz
  mv -f docker-compose.hyper.yml docker-compose.yml
  cat docker-compose.yml
  ./hyper config --accesskey ${HYPER_ACCESS} --secretkey ${HYPER_SECRET} --default-region "us-west-1"
  ./hyper compose down -p tetsujin
  ./hyper pull matoba/tetsujin
  ./hyper pull matoba/cdcc
  ./hyper compose up -d -p tetsujin
  ./hyper rmi $(./hyper images -f "dangling=true" -q)
else
  echo "$TRAVIS_BRANCH branch"
fi
