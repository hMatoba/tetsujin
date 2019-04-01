# !/bin/sh -x
if [ "$TRAVIS_BRANCH" = "master" ] && [ "$TRAVIS_PULL_REQUEST" = "false" ]; then
  docker tag matoba/tetsujin:dev matoba/tetsujin:latest
  docker login -u="${DOCKER_USERNAME}" -p="${DOCKER_PASSWORD}"
  docker push matoba/tetsujin:latest
  docker tag matoba/tetsujin:latest matoba/tetsujin:${TRAVIS_BUILD_NUMBER}
  docker push matoba/tetsujin:${TRAVIS_BUILD_NUMBER}
else
  echo "$TRAVIS_BRANCH branch"
fi
