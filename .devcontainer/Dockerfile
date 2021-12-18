# See here for image contents: https://github.com/microsoft/vscode-dev-containers/tree/v0.209.5/containers/alpine/.devcontainer/base.Dockerfile

# [Choice] Alpine version: 3.14, 3.13, 3.12, 3.11
ARG VARIANT="3.15"
FROM mcr.microsoft.com/vscode/devcontainers/base:0-alpine-${VARIANT}
#FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14

# Ensure APK up-to-date before beginning
RUN apk update

# V8 build dependencies
RUN apk add --no-cache bash make clang pkgconf

# Install python/pip
ENV PYTHONUNBUFFERED=1
RUN apk add --update --no-cache python3 && ln -sf python3 /usr/bin/python
RUN python3 -m ensurepip
RUN pip3 install --no-cache --upgrade pip setuptools

# dotnet dependencies
RUN apk add --no-cache icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib 

# RUN apk add libgdiplus --repository https://dl-3.alpinelinux.org/alpine/edge/testing/

# gcompat for linking v8
RUN apk add gcompat libpthread-stubs

# Install dotnet sdk
RUN export DOTNET_FILE=dotnet-sdk-6.0.100-linux-musl-x64.tar.gz && \
    export DOTNET_ROOT=/opt/dotnet && \
    wget https://download.visualstudio.microsoft.com/download/pr/bb523fba-7eb0-49ff-8214-c78c65dae090/7e7f9798ee57bf93649ada3eb13a79ae/$DOTNET_FILE && \
    mkdir -p "$DOTNET_ROOT" && tar zxf "$DOTNET_FILE" -C "$DOTNET_ROOT"

# Add debuggers
RUN apk add gdb lldb 

# Add Musl
RUN apk add musl

# Add gcc/g++
RUN apk add build-base

# Set path for dotnet globally
ENV PATH="/opt/dotnet:${PATH}"
