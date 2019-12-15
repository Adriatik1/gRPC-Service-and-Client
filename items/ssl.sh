#!/bin/bash
# Inspired from: https://github.com/grpc/grpc-java/tree/master/examples#generating-self-signed-certificates-for-use-with-grpc

# Output files
# ca.key: Certificate Authority private key file (this shouldn't be shared in real-life)
# ca.crt: Certificate Authority trust certificate (this should be shared with users in real-life)
# server.key: Server private key, password protected (this shouldn't be shared)
# server.csr: Server certificate signing request (this should be shared with the CA owner)
# server.crt: Server certificate signed by the CA (this would be sent back by the CA owner) - keep on server
# server.pem: Conversion of server.key into a format gRPC likes (this shouldn't be shared)

# Summary 
# Private files: ca.key, server.key, server.pem, server.crt
# "Share" files: ca.crt (needed by the client), server.csr (needed by the CA)

@echo off
SERVER_CN=localhost
set OPENSSL_CONF=c:\OpenSSL-Win64\bin\openssl.cfg

echo Generate CA key:
openssl genrsa -passout pass:1111 -des3 -out ca.key 4096

echo Generate CA certificate:
openssl req -passin pass:1111 -new -x509 -days 365 -key ca.key -out ca.crt -subj  "//CN=MyRootCA"

echo Generate server key:
openssl genrsa -passout pass:1111 -des3 -out server.key 4096

echo Generate server signing request:
openssl req -passin pass:1111 -new -key server.key -out server.csr -subj  "//CN=${SERVER_CN}"

echo Self-sign server certificate:
openssl x509 -req -passin pass:1111 -days 365 -in server.csr -CA ca.crt -CAkey ca.key -set_serial 01 -out server.crt

echo Remove passphrase from server key:
openssl rsa -passin pass:1111 -in server.key -out server.key

echo Generate client key
openssl genrsa -passout pass:1111 -des3 -out client.key 4096

echo Generate client signing request:
openssl req -passin pass:1111 -new -key client.key -out client.csr -subj  "//CN=${SERVER_CN}"

echo Self-sign client certificate:
openssl x509 -passin pass:1111 -req -days 365 -in client.csr -CA ca.crt -CAkey ca.key -set_serial 01 -out client.crt

echo Remove passphrase from client key:
openssl rsa -passin pass:1111 -in client.key -out client.key

cp ca.crt ../GrpcServer/ssl/ca.crt
cp ca.crt ../GrpcClient/ssl/ca.crt

cp server.crt ../GrpcServer/ssl/server.crt
cp server.key ../GrpcServer/ssl/server.key

cp client.crt ../GrpcClient/ssl/client.crt
cp client.key ../GrpcClient/ssl/client.key