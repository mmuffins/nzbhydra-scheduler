#!/bin/bash

/usr/scheduler/publish/nzbhydra-schedule getsearchterms -e $GROUPSFILE -w $SHOWSFILE -l $LOGLEVEL -s $SEARCHTERMSFILE -o $NZBOUTPUTDIRECTORY -u $NZBHYDRAURI -k $NZBHYDRAAPIKEY -a $MAXAGE -c $CATEGORY -i $INDEXERS -n $MINSIZE -m $MAXSIZE -q $REQUESTCOOLDOWN  > /proc/1/fd/1 2>/proc/1/fd/2