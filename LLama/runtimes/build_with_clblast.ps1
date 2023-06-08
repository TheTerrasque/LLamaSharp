# From https://github.com/ggerganov/llama.cpp/blob/master/.github/workflows/build.yml
git clone https://github.com/ggerganov/llama.cpp.git
cd llama.cpp

$CLBLAST_FOLDER = "$PWD\build\CLBlast"
$CLBLAST_VERSION = "1.6.0"
$OPENCL_VERSION =  "2023.04.17"
$CMAKE_ARGS = "-DLLAMA_CLBLAST=ON -DCMAKE_PREFIX_PATH=`"$($CLBLAST_FOLDER.Replace('\','/'))`""

Remove-Item $CLBLAST_FOLDER -Recurse -Force -ErrorAction Ignore
mkdir $CLBLAST_FOLDER

curl.exe -o $CLBLAST_FOLDER/opencl.zip -L "https://github.com/KhronosGroup/OpenCL-SDK/releases/download/v${OPENCL_VERSION}/OpenCL-SDK-v${OPENCL_VERSION}-Win-x64.zip"
mkdir $CLBLAST_FOLDER/opencl
tar.exe -xvf $CLBLAST_FOLDER/opencl.zip --strip-components=1 -C $CLBLAST_FOLDER/opencl

curl.exe -o $CLBLAST_FOLDER/clblast.7z -L "https://github.com/CNugteren/CLBlast/releases/download/${CLBLAST_VERSION}/CLBlast-${CLBLAST_VERSION}-windows-x64.7z"
curl.exe -o $CLBLAST_FOLDER/CLBlast.LICENSE.txt -L "https://github.com/CNugteren/CLBlast/raw/${CLBLAST_VERSION}/LICENSE"
7z x "-o${CLBLAST_FOLDER}" $CLBLAST_FOLDER/clblast.7z
rename-item $CLBLAST_FOLDER/CLBlast-${CLBLAST_VERSION}-windows-x64 clblast
foreach ($f in (Get-ChildItem -Recurse -Path "$CLBLAST_FOLDER/clblast" -Filter '*.cmake')) {
    $txt = Get-Content -Path $f.FullName -Raw
    $txt.Replace('C:/vcpkg/packages/opencl_x64-windows/', "$($CLBLAST_FOLDER.Replace('\','/'))/opencl/") | Set-Content -Path $f.FullName -Encoding UTF8
}

$blast_build=$($CLBLAST_FOLDER.Replace('\','/'))
Set-Location build
Write-Host cmake .. -DBUILD_SHARED_LIBS=ON $CMAKE_ARGS
cmake .. -DBUILD_SHARED_LIBS=ON -DLLAMA_CLBLAST=ON -DCMAKE_PREFIX_PATH="$blast_build"
cmake --build . --config Release 

Copy-Item -Path "$CLBLAST_FOLDER/clblast/lib/clblast.dll" -Destination "bin/Release/clblast.dll"

cd ..\..

cp "llama.cpp/build/bin/Release/clblast.dll" .
cp "llama.cpp/build/bin/Release/llama.dll" .

Remove-Item llama.cpp -Recurse -Force -ErrorAction Ignore