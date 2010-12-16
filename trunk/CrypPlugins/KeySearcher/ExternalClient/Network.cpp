#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <sstream>
#include <unistd.h>
#include <sys/types.h>
#include <arpa/inet.h>
#include <sys/sysctl.h>
#include <iostream>

#include <sys/types.h>
#include <sys/sysctl.h>



#include "PlatformIndependentWrapper.h"
#include "Opcodes.h"
#include "Job.h"
#include "Cryptool.h"

Cryptool* cryptool = 0;

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
    if (cryptool == 0)
        cryptool = new Cryptool();

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
                    j.KeySize = wrapper.ReadInt();
                    j.Key = new char[j.KeySize];
                    wrapper.ReadArray(j.Key, j.KeySize);
                    j.LargerThen = (wrapper.ReadInt() ? true : false);
                    j.Size = wrapper.ReadInt();
                    j.ResultSize = wrapper.ReadInt();
                    printf("Got new job! guid=%s\n", j.Guid.c_str());

                    JobResult res = cryptool->doOpenCLJob(j);

		    //send results back:
                    wrapper.WriteInt(ClientOpcodes::JOB_RESULT);
                    wrapper.WriteString(j.Guid);
                    wrapper.WriteInt(res.ResultList.size());
                    for (std::list<std::pair<float, int> >::iterator it = res.ResultList.begin(); it != res.ResultList.end(); it++)
                    {
                        wrapper.WriteInt(it->second);
                        wrapper.WriteFloat(it->first);
                    }
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

