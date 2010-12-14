#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <string.h>
#include <netinet/in.h>
#include <arpa/inet.h>

#include "Network.h"

#define RECONNECT_TIME  5

void usage(char **argv)
{
    printf("Usage: %s $THREAD_NUM $HOST $PORT\n\n", argv[0]);
    printf("    THREAD_NUM - Number of concurrent threads\n");
    printf("    HOST       - PC running cryptool\n");
    printf("    PORT       - Cryptool's listening port\n");
}

int threadNum;


int main (int argc, char **argv)
{
    if(argc != 4)
    {
        usage(argv);
        return 1;
    }

    threadNum = atoi(argv[1]);
    if(threadNum== 0)
    {
        printf("Invalid number of threads\n");
        return 1;
    }

    hostent *server = gethostbyname(argv[2]);
    if(!server)
    {
        printf("Invalid host\n");
        return 1;
    }

    int port = atoi(argv[3]);

    if(port == 0 || port <= 0 || port > 0xFFFF)
    {
        printf("Invalid port.\n");
        return 1;
    }

    sockaddr_in serv_addr;
    bzero((char *) &serv_addr, sizeof(serv_addr));
    serv_addr.sin_family = AF_INET;
    bcopy((char *)server->h_addr, 
            (char *)&serv_addr.sin_addr.s_addr,
            server->h_length);
    serv_addr.sin_port = htons(port);
    while(true)
    {
        networkThread(serv_addr, port);
        printf("Reconnecting in %u seconds\n", RECONNECT_TIME);
        sleep(RECONNECT_TIME);
    }

}
