SDK := $(HOME)/src/ati-stream-sdk-v2.2-lnx32

OBJFILES := $(patsubst %.cpp,bin/%.o,$(wildcard *.cpp))

CCFLAGS := -g -Wall -Wextra -fopenmp -Wfloat-equal -Wpointer-arith
#CCFLAGS := -O3 -g -Wall -Wextra -fopenmp -Wfloat-equal -Wpointer-arith

.PHONY: all clean

all: bin/Cryptool

bin/Cryptool: $(OBJFILES)
	g++ $(CCFLAGS) -o bin/Cryptool bin/*.o -lpthread -ldl -L/usr/X11R6/lib -lOpenCL -lSDKUtil -L$(SDK)/lib/x86 -L$(SDK)/TempSDKUtil/lib/x86 

bin/%.o: %.cpp
	mkdir -p bin
	g++ $(CCFLAGS) -DATI_OS_LINUX -I $(SDK)/include -I $(SDK)/samples/opencl/SDKUtil/include -o $@ -c $<

clean:
	rm -r bin
