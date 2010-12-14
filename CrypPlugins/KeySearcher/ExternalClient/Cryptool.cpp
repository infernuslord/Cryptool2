/* ============================================================

Copyright (c) 2009 Advanced Micro Devices, Inc.  All rights reserved.
 
Redistribution and use of this material is permitted under the following 
conditions:
 
Redistributions must retain the above copyright notice and all terms of this 
license.
 
In no event shall anyone redistributing or accessing or using this material 
commence or participate in any arbitration or legal action relating to this 
material against Advanced Micro Devices, Inc. or any copyright holders or 
contributors. The foregoing shall survive any expiration or termination of 
this license or any agreement or access or use related to this material. 

ANY BREACH OF ANY TERM OF THIS LICENSE SHALL RESULT IN THE IMMEDIATE REVOCATION 
OF ALL RIGHTS TO REDISTRIBUTE, ACCESS OR USE THIS MATERIAL.

THIS MATERIAL IS PROVIDED BY ADVANCED MICRO DEVICES, INC. AND ANY COPYRIGHT 
HOLDERS AND CONTRIBUTORS "AS IS" IN ITS CURRENT CONDITION AND WITHOUT ANY 
REPRESENTATIONS, GUARANTEE, OR WARRANTY OF ANY KIND OR IN ANY WAY RELATED TO 
SUPPORT, INDEMNITY, ERROR FREE OR UNINTERRUPTED OPERA TION, OR THAT IT IS FREE 
FROM DEFECTS OR VIRUSES.  ALL OBLIGATIONS ARE HEREBY DISCLAIMED - WHETHER 
EXPRESS, IMPLIED, OR STATUTORY - INCLUDING, BUT NOT LIMITED TO, ANY IMPLIED 
WARRANTIES OF TITLE, MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, 
ACCURACY, COMPLETENESS, OPERABILITY, QUALITY OF SERVICE, OR NON-INFRINGEMENT. 
IN NO EVENT SHALL ADVANCED MICRO DEVICES, INC. OR ANY COPYRIGHT HOLDERS OR 
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, PUNITIVE,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, REVENUE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED OR BASED ON ANY THEORY OF LIABILITY 
ARISING IN ANY WAY RELATED TO THIS MATERIAL, EVEN IF ADVISED OF THE POSSIBILITY 
OF SUCH DAMAGE. THE ENTIRE AND AGGREGATE LIABILITY OF ADVANCED MICRO DEVICES, 
INC. AND ANY COPYRIGHT HOLDERS AND CONTRIBUTORS SHALL NOT EXCEED TEN DOLLARS 
(US $10.00). ANYONE REDISTRIBUTING OR ACCESSING OR USING THIS MATERIAL ACCEPTS 
THIS ALLOCATION OF RISK AND AGREES TO RELEASE ADVANCED MICRO DEVICES, INC. AND 
ANY COPYRIGHT HOLDERS AND CONTRIBUTORS FROM ANY AND ALL LIABILITIES, 
OBLIGATIONS, CLAIMS, OR DEMANDS IN EXCESS OF TEN DOLLARS (US $10.00). THE 
FOREGOING ARE ESSENTIAL TERMS OF THIS LICENSE AND, IF ANY OF THESE TERMS ARE 
CONSTRUED AS UNENFORCEABLE, FAIL IN ESSENTIAL PURPOSE, OR BECOME VOID OR 
DETRIMENTAL TO ADVANCED MICRO DEVICES, INC. OR ANY COPYRIGHT HOLDERS OR 
CONTRIBUTORS FOR ANY REASON, THEN ALL RIGHTS TO REDISTRIBUTE, ACCESS OR USE 
THIS MATERIAL SHALL TERMINATE IMMEDIATELY. MOREOVER, THE FOREGOING SHALL 
SURVIVE ANY EXPIRATION OR TERMINATION OF THIS LICENSE OR ANY AGREEMENT OR 
ACCESS OR USE RELATED TO THIS MATERIAL.

NOTICE IS HEREBY PROVIDED, AND BY REDISTRIBUTING OR ACCESSING OR USING THIS 
MATERIAL SUCH NOTICE IS ACKNOWLEDGED, THAT THIS MATERIAL MAY BE SUBJECT TO 
RESTRICTIONS UNDER THE LAWS AND REGULATIONS OF THE UNITED STATES OR OTHER 
COUNTRIES, WHICH INCLUDE BUT ARE NOT LIMITED TO, U.S. EXPORT CONTROL LAWS SUCH 
AS THE EXPORT ADMINISTRATION REGULATIONS AND NATIONAL SECURITY CONTROLS AS 
DEFINED THEREUNDER, AS WELL AS STATE DEPARTMENT CONTROLS UNDER THE U.S. 
MUNITIONS LIST. THIS MATERIAL MAY NOT BE USED, RELEASED, TRANSFERRED, IMPORTED,
EXPORTED AND/OR RE-EXPORTED IN ANY MANNER PROHIBITED UNDER ANY APPLICABLE LAWS, 
INCLUDING U.S. EXPORT CONTROL LAWS REGARDING SPECIFICALLY DESIGNATED PERSONS, 
COUNTRIES AND NATIONALS OF COUNTRIES SUBJECT TO NATIONAL SECURITY CONTROLS. 
MOREOVER, THE FOREGOING SHALL SURVIVE ANY EXPIRATION OR TERMINATION OF ANY 
LICENSE OR AGREEMENT OR ACCESS OR USE RELATED TO THIS MATERIAL.

NOTICE REGARDING THE U.S. GOVERNMENT AND DOD AGENCIES: This material is 
provided with "RESTRICTED RIGHTS" and/or "LIMITED RIGHTS" as applicable to 
computer software and technical data, respectively. Use, duplication, 
distribution or disclosure by the U.S. Government and/or DOD agencies is 
subject to the full extent of restrictions in all applicable regulations, 
including those found at FAR52.227 and DFARS252.227 et seq. and any successor 
regulations thereof. Use of this material by the U.S. Government and/or DOD 
agencies is acknowledgment of the proprietary rights of any copyright holders 
and contributors, including those of Advanced Micro Devices, Inc., as well as 
the provisions of FAR52.227-14 through 23 regarding privately developed and/or 
commercial computer software.

This license forms the entire agreement regarding the subject matter hereof and 
supersedes all proposals and prior discussions and writings between the parties 
with respect thereto. This license does not affect any ownership, rights, title,
or interest in, or relating to, this material. No terms of this license can be 
modified or waived, and no breach of this license can be excused, unless done 
so in a writing signed by all affected parties. Each term of this license is 
separately enforceable. If any term of this license is determined to be or 
becomes unenforceable or illegal, such term shall be reformed to the minimum 
extent necessary in order for this license to remain in effect in accordance 
with its terms as modified by such reformation. This license shall be governed 
by and construed in accordance with the laws of the State of Texas without 
regard to rules on conflicts of law of any state or jurisdiction or the United 
Nations Convention on the International Sale of Goods. All disputes arising out 
of this license shall be subject to the jurisdiction of the federal and state 
courts in Austin, Texas, and all defenses are hereby waived concerning personal 
jurisdiction and venue of these courts.

============================================================ */


//
// Copyright (c) 2009 Advanced Micro Devices, Inc. All rights reserved.
//

#include <cstdio>
#include <cstdlib>
#include <iostream>
#include <SDKFile.hpp>
#include <SDKCommon.hpp>
#include <SDKApplication.hpp>

#define __NO_STD_VECTOR
#define __NO_STD_STRING

#include <CL/cl.hpp>

#include "Job.h"
int doOpenCLJob(const Job& j)
{
    cl_int err;

    // Platform info
    cl::vector<cl::Platform> platforms;
    std::cout<<"HelloCL!\nGetting Platform Information\n";
    err = cl::Platform::get(&platforms);
    if(err != CL_SUCCESS)
    {
        std::cerr << "Platform::get() failed (" << err << ")" << std::endl;
        return SDK_FAILURE;
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
        return SDK_FAILURE;
    }

    /* 
     * If we could find our platform, use it. Otherwise pass a NULL and get whatever the
     * implementation thinks we should be using.
     */

    cl_context_properties cps[3] = { CL_CONTEXT_PLATFORM, (cl_context_properties)(*i)(), 0 };

    std::cout<<"Creating a context AMD platform\n";
    cl::Context context(CL_DEVICE_TYPE_CPU, cps, NULL, NULL, &err);
    if (err != CL_SUCCESS) {
        std::cerr << "Context::Context() failed (" << err << ")\n";
        return SDK_FAILURE;
    }

    std::cout<<"Getting device info\n";
    cl::vector<cl::Device> devices = context.getInfo<CL_CONTEXT_DEVICES>();
    if (err != CL_SUCCESS) {
        std::cerr << "Context::getInfo() failed (" << err << ")\n";
        return SDK_FAILURE;
    }
    if (devices.size() == 0) {
        std::cerr << "No device available\n";
        return SDK_FAILURE;
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
    std::cout<<"compiling CL source\n";
	cl::Program::Sources sources(1, std::make_pair(j.Src.c_str(), j.Src.length()));

	cl::Program program = cl::Program(context, sources, &err);
	if (err != CL_SUCCESS) {
        std::cerr << "Program::Program() failed (" << err << ")\n";
        return SDK_FAILURE;
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
        return SDK_FAILURE;
    }

    cl::Kernel kernel(program, "bruteforceKernel", &err);
    if (err != CL_SUCCESS) {
        std::cerr << "Kernel::Kernel() failed (" << err << ")\n";
        return SDK_FAILURE;
    }

    cl::CommandQueue queue(context, devices[0], 0, &err);
    if (err != CL_SUCCESS) {
        std::cerr << "CommandQueue::CommandQueue() failed (" << err << ")\n";
    }

    // key
    const char* strkey = "lol";

    cl::Buffer keybuffer = cl::Buffer(context, CL_MEM_READ_ONLY, sizeof(strkey), NULL, &err);
    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed to allocate keybuffer(" << err << ")\n";
        return SDK_FAILURE;
    }

    err = queue.enqueueWriteBuffer(keybuffer, 1, 0, sizeof(strkey), strkey);

    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed write to keybuffer(" << err << ")\n";
        return SDK_FAILURE;
    }

    err = kernel.setArg(0, keybuffer);
    if (err != CL_SUCCESS) {
        std::cerr << "Kernel::setArg() failed (" << err << ")\n";
        return SDK_FAILURE;
    }

    // results
    cl::Buffer entropies = cl::Buffer(context, CL_MEM_WRITE_ONLY, sizeof(float)*20, NULL, &err);

    if(err != CL_SUCCESS)
    {
        std::cerr << "Failed allocate to entropybuffer(" << err << ")\n";
        return SDK_FAILURE;
    }

    err = kernel.setArg(1, entropies);

    if (err != CL_SUCCESS) {
        std::cerr << "Kernel::setArg() failed (" << err << ")\n";
        return SDK_FAILURE;
    }


    std::cout<<"Running CL program\n";
    err = queue.enqueueNDRangeKernel(
        kernel, cl::NullRange, cl::NDRange(4, 4), cl::NDRange(2, 2)
    );

    if (err != CL_SUCCESS) {
        std::cerr << "CommandQueue::enqueueNDRangeKernel()" \
            " failed (" << err << ")\n";
       return SDK_FAILURE;
    }

    err = queue.finish();
    if (err != CL_SUCCESS) {
        std::cerr << "Event::wait() failed (" << err << ")\n";
        return SDK_FAILURE;
    }

    std::cout<<"Done\nPassed!\n";

    float localEntropy[20] = {};

    queue.enqueueReadBuffer(entropies, 1, 0, sizeof(float)*20, localEntropy);

    for(int i=0; i<20; ++i)
    {
        printf("Entropy[%i]=%f\n", i, localEntropy[i]);
    }
    return SDK_SUCCESS;
}
