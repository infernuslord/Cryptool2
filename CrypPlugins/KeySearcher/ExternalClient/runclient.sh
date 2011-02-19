#!/bin/bash
export LD_LIBRARY_PATH=~/src/ati-stream-sdk-v2.2-lnx32/lib/x86/:.
# some vendors install an incompatbile own libGL
export LD_PRELOAD=/usr/lib/libGL.so
make && ./bin/Cryptool 192.168.1.109 6235 123
