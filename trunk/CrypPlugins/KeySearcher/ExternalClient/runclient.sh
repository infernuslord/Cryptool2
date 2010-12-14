#!/bin/bash
export LD_LIBRARY_PATH=~/src/ati-stream-sdk-v2.2-lnx32/lib/x86/:.
make
./bin/Cryptool 2 localhost 4154
