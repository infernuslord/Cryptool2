This is a modified version of msieve-1.42 (http://sourceforge.net/projects/msieve),
which can be used in .NET.

Please compile it with Visual Studio 2008.

To compile this, you first have to compile mpir (MPIR is an 
open source multiprecision integer library derived from version 4.2.1 
of the GMP (GNU Multi Precision) project).

We recommend using mpir-1.2.1.
Get it here: http://www.mpir.org/mpir-1.2.1.tar.gz

You also have to compile  (http://gforge.inria.fr/frs/download.php/22124/ecm-6.2.3.tar.gz).

Edit the build.vc9\mpir_config.vsprops file to set the path to these libraries.


You have to change the C-Runtimelibrary of these libraries to "Multithreaded-XX-DLL", or else you will 
experience problems while linking msieve.


If you encounter problems, don't hesitate to ask me (svenrech at googlemail dot com).