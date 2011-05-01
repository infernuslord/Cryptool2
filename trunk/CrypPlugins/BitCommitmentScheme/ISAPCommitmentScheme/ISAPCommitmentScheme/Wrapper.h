// ISAPCommitmentScheme.h

#pragma once

#include <iostream>
#include <sstream>

using namespace System;

extern int main(int argc, char** argv);		//this is the entry point to the ISAPCommitmentScheme implementation

namespace ISAPCommitmentScheme {

	public ref class Wrapper
	{
	public:
		String^ Run(bool input)
		{
			//redirect cout output:
			std::stringstream redirectedOutput;
			std::cout.rdbuf(redirectedOutput.rdbuf());

			//calling main method:
			char** argv = new char*[2];
			argv[0] = "";			
			argv[1] = input ? "1" : "0";
			int err = main(2, argv);
			if (err != 0)
			{
				throw gcnew Exception(gcnew String(String::Format("ISAPCommitmentScheme failed with error code {0}!", err)));
			}

			std::string output = redirectedOutput.str();
			return gcnew String(output.c_str());
		}
	};
}
