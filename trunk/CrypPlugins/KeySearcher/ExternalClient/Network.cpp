#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <sstream>
#include <unistd.h>
#include <sys/types.h>
#include <arpa/inet.h>
#include <sys/sysctl.h>

#include <sys/types.h>
#include <sys/sysctl.h>



#include "PlatformIndependentWrapper.h"
#include "Opcodes.h"
#include "Job.h"
#include "Cryptool.h"

std::string getIdentificationStr()
{
    std::stringstream out;
    out << "cores:";
    out << sysconf( _SC_NPROCESSORS_ONLN );
    // todo: more
    return out.str();
}

void GetJobsAndPostResults(PlatformIndependentWrapper wrapper)
{
    wrapper.WriteInt(ClientOpcodes::HELLO);
    wrapper.WriteString(getIdentificationStr());

    // loop will be escaped by wrapper exceptions
    while(true)
    {
        switch(wrapper.ReadInt())
        {
            case ServerOpcodes::NEW_JOB:
                {
                    Job j;
                    j.Guid = wrapper.ReadString();
                    j.Src = wrapper.ReadString();
                    j.Input= wrapper.ReadString();
                    printf("Got new job! guid=%s\n", j.Guid.c_str());

                    doOpenCLJob(j);

                    wrapper.WriteInt(ClientOpcodes::JOB_RESULT);
                    wrapper.WriteString(j.Guid);
                    wrapper.WriteString("not founds anythings :(");
                }
                break;
        }
    }
}

void networkThread(sockaddr_in serv_addr, int port)
{
    printf("Connecting to %s on port %i\n", inet_ntoa(serv_addr.sin_addr), port);
    int sockfd = socket(AF_INET, SOCK_STREAM, 0);

    if (sockfd < 0) 
    {
        printf("ERROR opening socket\n");
        return;
    }
    printf("Connecting established\n");
    if (connect(sockfd, (sockaddr*)&serv_addr, sizeof(serv_addr)) < 0)
    {
        printf("Couldn't connect\n");
        close(sockfd);
        return;
    }

    try{
        PlatformIndependentWrapper w(sockfd);
        GetJobsAndPostResults(w);
    } catch(SocketException)
    {
        close(sockfd);
        return;
    }
}

