cd /d %~dp0

%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe Deployment\ResetDatabase.xml /t:Fire 

pushd ..\Assemblies\ElasticSearchVNext\Server\bin\
start elasticsearch.bat
popd

pushd ..\Assemblies\Cassandra\Server\bin\
start cassandra.bat
popd

start "ServiceRunner" "..\Tools.Compiled\ServiceRunner\ServiceRunner.exe" "_StartAllConfigs\startAll.SR.yaml" "-startAllServices"