#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <sstream>
#include <unistd.h>
#include <sys/types.h>
#include <arpa/inet.h>
#include <sys/sysctl.h>
#include <iostream>
#include <queue>

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


void writeJobResult(PlatformIndependentWrapper& wrapper, JobResult& result)
{
    wrapper.WriteInt(ClientOpcodes::JOB_RESULT);
    wrapper.WriteString(result.Guid);
    wrapper.WriteInt(result.ResultList.size());
    for (std::list<std::pair<float, int> >::iterator it = result.ResultList.begin(); it != result.ResultList.end(); ++it)
    {
        wrapper.WriteInt(it->second);
        wrapper.WriteFloat(it->first);
    }
}

// Queue of completed jobs
std::queue<JobResult> finishedJobs;
void GetJobsAndPostResults(PlatformIndependentWrapper& wrapper)
{
    if (cryptool == 0)
        cryptool = new Cryptool();

    wrapper.WriteInt(ClientOpcodes::HELLO);
    wrapper.WriteString(getIdentificationStr());

    while(!finishedJobs.empty())
    {
        printf("Trying to send %u finished job%s\n", finishedJobs.size(), finishedJobs.size()>1?"s":"");
        writeJobResult(wrapper, finishedJobs.front());
        finishedJobs.pop();
    }
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
                    try {
                        writeJobResult(wrapper, res);
                    }catch(SocketException e)
                    {
                        printf("Exception while writing results :(\n");
                        throw e;
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

    if (connect(sockfd, (sockaddr*)&serv_addr, sizeof(serv_addr)) < 0)
    {
        printf("Couldn't connect\n");
        close(sockfd);
        return;
    }
    printf("Connection established\n");

    try{
        PlatformIndependentWrapper w(sockfd);
        GetJobsAndPostResults(w);
    } catch(SocketException)
    {
        close(sockfd);
        return;
    }
}

