#pragma once

#include <CL/cl.hpp>
#include "Job.h"

class Cryptool
{
    private:
	cl::vector<cl::Device> devices;
	cl::Context* context;
	cl::Kernel* kernel;

	void buildKernel(const Job& j);
	void pushInTop(std::list<std::pair<float, int> >& top, std::list<std::pair<float, int> >::iterator it, float val, int k);
	std::list<std::pair<float, int> >::iterator isInTop(std::list<std::pair<float, int> >& top, float val, bool LargerThen);
	void initTop(std::list<std::pair<float, int> >& top, bool LargerThen);
    public:
	Cryptool();
	JobResult doOpenCLJob(const Job& j);
};
