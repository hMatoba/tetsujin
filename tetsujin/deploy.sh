# !/bin/sh -x
if [ "$TRAVIS_BRANCH" = "master" ] && [ "$TRAVIS_PULL_REQUEST" = "false" ]; then
  docker tag matoba/tetsujin:dev matoba/tetsujin:latest
  docker login -u="${DOCKER_USERNAME}" -p="${DOCKER_PASSWORD}"
  docker push matoba/tetsujin:latest
  docker tag matoba/tetsujin:latest matoba/tetsujin:${TRAVIS_BUILD_NUMBER}
  docker push matoba/tetsujin:${TRAVIS_BUILD_NUMBER}
  wget https://hyper-install.s3.amazonaws.com/hyper-linux-x86_64.tar.gz
  tar xzf hyper-linux-x86_64.tar.gz
  mv -f _docker-compose.hyper.yml docker-compose.yml
  cat docker-compose.yml
  ./hyper config --accesskey ${HYPER_ACCESS} --secretkey ${HYPER_SECRET} --default-region "us-west-1"
  ./hyper compose down -p tetsujin
  ./hyper pull matoba/tetsujin
  ./hyper pull matoba/cdcc
  ./hyper compose up -d -p tetsujin
  ./hyper rmi $(./hyper images -f "dangling=true" -q)
  ./hyper ps
else
  echo "$TRAVIS_BRANCH branch"
fi
