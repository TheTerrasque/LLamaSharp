Needs a llama.cpp dll or .so file to run. Those can be built from the llama.cpp repository by using -DBUILD_SHARED_LIBS=ON when building.

The build_with_clblast.ps1 will download and build llama.cpp with clblast. It assumes these programs are installed:

* Git for Windows - https://git-scm.com/download/win
* Cmake for Windows - https://cmake.org/download/
 * Valid build tool for Cmake
    * Visual Studio 2015 or 2017 (Community Edition is fine) with C++ support (https://www.visualstudio.com/downloads/)
    * Possibly MinGW-w64 (http://mingw-w64.org/doku.php/download/mingw-builds)
* 7zip - http://www.7-zip.org/download.html