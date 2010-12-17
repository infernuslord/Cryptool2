#include <cstdio>
#include <cstdlib>
#include <iostream>
#include <SDKFile.hpp>
#include <SDKCommon.hpp>
#include <SDKApplication.hpp>

#define __NO_STD_VECTOR
#define __NO_STD_STRING

#include "Cryptool.h"

Cryptool::Cryptool()
{
    cl_int err;

    kernel = 0;

    // Platform info
    cl::vector<cl::Platform> platforms;
    std::cout<<"HelloCL!\nGetting Platform Information\n";
    err = cl::Platform::get(&platforms);
    if(err != CL_SUCCESS)
    {
        std::cerr << "Platform::get() failed (" << err << ")" << std::endl;
        throw std::exception();
    }
    if (platforms.size() == 0)
    {
        std::cerr << "No platforms available!" << std::endl;
        throw std::exception();
    }

    cl::vector<cl::Platform>::iterator i;
    if(platforms.size() > 0)
    {
        for(i = platforms.begin(); i != platforms.end(); ++i)
        {
            if(!strcmp((*i).getInfo<CL_PLATFORM_VENDOR>(&err).c_str(), "Advanced Micro Devices, Inc."))
            {
                break;
            }
        }
    }
    if(err != CL_SUCCESS)
    {
        std::cerr << "Platform::getInfo() failed (" << err << ")" << std::endl;
        throw std::exception();
    }

    /* 
     * If we could find our platform, use it. Otherwise pass a NULL and get whatever the
     * implementation thinks we should be using.
     */

    cl_context_properties cps[3] = { CL_CONTEXT_PLATFORM, (cl_context_properties)(*i)(), 0 };

    std::cout<<"Creating a context AMD platform\n";
    context = new cl::Context(CL_DEVICE_TYPE_CPU, cps, NULL, NULL, &err);
    if (err != CL_SUCCESS) {
        std::cerr << "Context::Context() failed (" << err << ")\n";
        throw std::exception();
    }

    std::cout<<"Getting device info\n";
    devices = context->getInfo<CL_CONTEXT_DEVICES>();
    if (err != CL_SUCCESS) {
        std::cerr << "Context::getInfo() failed (" << err << ")\n";
        throw std::exception();
    }
    if (devices.size() == 0) {
        std::cerr << "No device available\n";
        throw std::exception();
    }

    for(uint32_t i=0; i< devices.size(); ++i)
    {
        std::string out;
        devices[i].getInfo(CL_DEVICE_NAME, &out);
        printf("name: %s\n", out.c_str());
        devices[i].getInfo(CL_DEVICE_VENDOR, &out);
        printf("vendor: %s\n", out.c_str());
        devices[i].getInfo(CL_DEVICE_OPENCL_C_VERSION, &out);
        printf("version c: %s\n", out.c_str());
    }

    // results
    costs = cl::Buffer(*context, CL_MEM_WRITE_ONLY, sizeof(float)*subbatch, NULL, &err);

    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed allocate to costsbuffer(" << err << ")\n";
        throw new std::exception();
    }
    
    localCosts = new float[subbatch];
}

void Cryptool::buildKernel(const Job& j)
{
    if (j.Src == "")
    {
        if (kernel != 0)
            return;
        else
        {
            std::cout << "Source transmission failure!" << std::endl;
            throw new std::exception();
        }
    }

    cl_int err;

    std::cout<<"compiling CL source\n";
    cl::Program::Sources sources(1, std::make_pair(j.Src.c_str(), j.Src.length()));

    cl::Program program = cl::Program(*context, sources, &err);
    if (err != CL_SUCCESS) {
        std::cerr << "Program::Program() failed (" << err << ")\n";
        throw new std::exception();
    }

    err = program.build(devices);
    if (err != CL_SUCCESS) {

	if(err == CL_BUILD_PROGRAM_FAILURE)
        {
            cl::string str = program.getBuildInfo<CL_PROGRAM_BUILD_LOG>(devices[0]);

            std::cout << " \n\t\t\tBUILD LOG\n";
            std::cout << " ************************************************\n";
			std::cout << str.c_str() << std::endl;
            std::cout << " ************************************************\n";
        }

        std::cerr << "Program::build() failed (" << err << ")\n";
        throw new std::exception();
    }

    if (kernel != 0)
	delete kernel;

    kernel = new cl::Kernel(program, "bruteforceKernel", &err);
    if (err != CL_SUCCESS) {
        std::cerr << "Kernel::Kernel() failed (" << err << ")\n";
        throw new std::exception();
    }
}

JobResult Cryptool::doOpenCLJob(const Job& j)
{
    cl_int err;

    buildKernel(j);

    cl::CommandQueue queue(*context, devices[0], 0, &err);
    if (err != CL_SUCCESS) {
        std::cerr << "CommandQueue::CommandQueue() failed (" << err << ")\n";
        throw new std::exception();
    }

    // key
    cl::Buffer keybuffer = cl::Buffer(*context, CL_MEM_READ_ONLY, j.KeySize*sizeof(float), NULL, &err);
    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed to allocate keybuffer(" << err << ")\n";
        throw new std::exception();
    }

    err = queue.enqueueWriteBuffer(keybuffer, 1, 0, j.KeySize*sizeof(float), j.Key);
    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed write to keybuffer(" << err << ")\n";
        throw new std::exception();
    }

    res.ResultList.resize(j.ResultSize);
    initTop(res.ResultList, j.LargerThen);

    //execute:
    std::cout<<"Running CL program with " << j.Size << " calculations!\n";
    enqueueKernel(queue, j.Size, keybuffer, costs, j);

    std::cout<<"Done!\n";

    return res;
}

void Cryptool::enqueueSubbatch(cl::CommandQueue& queue, cl::Buffer& keybuffer, cl::Buffer& costs, int add, int length, const Job& j)
{
	cl_int err;

	err = kernel->setArg(0, keybuffer);
	if (err != CL_SUCCESS) {
		std::cerr << "Kernel::setArg() failed (" << err << ")\n";
		throw new std::exception();
	}

	err = kernel->setArg(1, costs);
	if (err != CL_SUCCESS) {
		std::cerr << "Kernel::setArg() failed (" << err << ")\n";
		throw new std::exception();
	}

	err = kernel->setArg(2, add);
	if (err != CL_SUCCESS) {
		std::cerr << "Kernel::setArg() failed (" << err << ")\n";
		throw new std::exception();
	}

	err = queue.enqueueNDRangeKernel(*kernel, cl::NullRange, cl::NDRange(256, 256), cl::NullRange);

	if (err != CL_SUCCESS) {
		std::cerr << "CommandQueue::enqueueNDRangeKernel()" \
		    " failed (" << err << ")\n";
		throw new std::exception();
	}

	err = queue.finish();
	if (err != CL_SUCCESS) {
		std::cerr << "Event::wait() failed (" << err << ")\n";
		throw new std::exception();
	}

	queue.enqueueReadBuffer(costs, 1, 0, sizeof(float)*length, localCosts);
	err = queue.finish();
	if (err != CL_SUCCESS) {
		std::cerr << "Event::wait() failed (" << err << ")\n";
		throw new std::exception();
	}

	//check results:
	for(int i=0; i<length; ++i)
	{
		//std::cout << localCosts[i] << std::endl;
		std::list<std::pair<float, int> >::iterator it = isInTop(res.ResultList, localCosts[i], j.LargerThen);
		if (it != res.ResultList.end())
			pushInTop(res.ResultList, it, localCosts[i], i+add);
	}
}

void Cryptool::enqueueKernel(cl::CommandQueue& queue, int size, cl::Buffer& keybuffer, cl::Buffer& costs, const Job& j)
{
    for (int i = 0; i < (size/subbatch); i++)
    {
	enqueueSubbatch(queue, keybuffer, costs, i*subbatch, subbatch, j);
    }

    int remain = (size%subbatch);
    if (remain != 0)
    {
	enqueueSubbatch(queue, keybuffer, costs, size-remain, remain, j);
    }
}

void Cryptool::pushInTop(std::list<std::pair<float, int> >& top, std::list<std::pair<float, int> >::iterator it, float val, int k) {
	top.insert(it, std::pair<float, int>(val, k));
	top.pop_back();
}

std::list<std::pair<float, int> >::iterator Cryptool::isInTop(std::list<std::pair<float, int> >& top, float val, bool LargerThen) {
        if (LargerThen)
	{
		for (std::list<std::pair<float, int> >::iterator k = top.begin(); k != top.end(); k++)
			if (val > k->first)
				return k;
	}
	else
	{
		for (std::list<std::pair<float, int> >::iterator k = top.begin(); k != top.end(); k++)
			if (val < k->first)
				return k;
	}

	return top.end();
}

void Cryptool::initTop(std::list<std::pair<float, int> >& top, bool LargerThen) {
	for (std::list<std::pair<float, int> >::iterator k = top.begin(); k != top.end(); k++)
        {
            if (LargerThen)
		k->first = -1000000.0;
            else
                k->first = 1000000.0;
        }
}
