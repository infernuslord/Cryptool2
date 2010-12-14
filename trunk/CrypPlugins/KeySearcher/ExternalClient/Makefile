SDK := $(HOME)/src/ati-stream-sdk-v2.2-lnx32

OBJFILES := $(patsubst %.cpp,bin/%.o,$(wildcard *.cpp))

.PHONY: all clean

all: bin/Cryptool

bin/Cryptool: $(OBJFILES)
	g++ -o bin/Cryptool bin/*.o -Wall -Wextra -lpthread -ldl -L/usr/X11R6/lib -lOpenCL -lSDKUtil -L$(SDK)/lib/x86 -L$(SDK)/TempSDKUtil/lib/x86 

bin/%.o: %.cpp
	mkdir -p bin
	g++ -msse3 -Wfloat-equal -Wall -Wextra -Wpointer-arith -DATI_OS_LINUX -g3 -ffor-scope -I $(SDK)/include -I $(SDK)/samples/opencl/SDKUtil/include -o $@ -c $<

clean:
	rm -r bin
