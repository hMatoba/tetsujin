FROM nginx:alpine
COPY nginx.conf /etc/nginx/nginx.conf

COPY server.crt /data/certs/server.crt
COPY sserver.key /data/certs/server.key

EXPOSE 443