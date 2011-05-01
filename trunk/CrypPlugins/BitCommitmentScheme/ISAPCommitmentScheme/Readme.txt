This C++/CLI project is a wrapper around the Bit Commitment Scheme code by Martin Schmidt.
The original source can be found at his website:
http://www.ifam.uni-hannover.de/~mschmidt/

To compile this project, you need the libraries MPIR and MPFR.
You can download MPIR at http://www.mpir.org.
You can download MPFR at http://www.mpfr.org/.
Additionally, you can download VS2010 project files for MPFR at http://gladman.plushost.co.uk/oldsite/computing/gmp4win.php.

Please also take a look into the Readme's of these projects.

Unpack these libraries in the BitCommitmentScheme and name their directories "mpir" and "mpfr".
After that, your directory structure should look like this:

    ISAPCommitmentScheme
        ISAPCommitmentScheme.sln
    
    mpir
        build.vc10
	    mpir.sln
            lib
            ....
    mpfr
        build.vc10
	    lib_mpfr.sln
            lib
            ....
	    
Now open mpir.sln with VS2010, select all projects from the solution and open their property dialog. Go to C/C++ -> Code Generation and set the "Runtime Library" setting to "Multithreaded-XX-DLL".
Now compile. Don't worry if it throws some errors. Just make sure that the mpirxx.lib was generated inside the mpir\build.vc10\lib\Win32\Debug or Release directory.

Now open lib_mpfr.sln and change the project runtime library setting to "Multithreaded-XX-DLL", too. Compile and make sure, that mpfr.lib was generated inside mpfr\build.vc10\lib\Win32\Debug or Release.

Now you can finally compile ISAPCommitmentScheme. It generates a DLL that will be copied into the AppReferences directory.