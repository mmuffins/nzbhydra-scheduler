#!/bin/bash

CRON_STRING="$CRON /usr/scheduler/publish/nzbhydra-schedule > /proc/1/fd/1 2>/proc/1/fd/2\n"
echo "Creating cron job $CRON_STRING"
echo -e "$CRON_STRING" > crontab
crontab crontab

echo "Starting cron..."
cron -f && tail -f /var/log/cron.log