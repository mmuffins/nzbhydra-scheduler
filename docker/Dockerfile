
FROM mcr.microsoft.com/dotnet/runtime:7.0

ENV DEBIAN_FRONTEND=noninteractive
RUN apt update && apt install -y dos2unix cron && rm -rf /var/lib/apt/lists/*

WORKDIR /usr/scheduler
COPY start.sh .
COPY publish/* ./publish/

# Fix line endings && execute permissions
RUN dos2unix *.sh \
    && \
    find . -type f -iname "*.sh" -exec chmod +x {} \;

RUN chmod 777 ./publish/nzbhydra-schedule

RUN touch /var/log/cron.log
CMD ["./start.sh"]
