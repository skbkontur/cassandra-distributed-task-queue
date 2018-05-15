cd /d %~dp0

%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe Deployment\ResetDatabase.xml /t:Fire 

call ..\MSBuild\DeployTargets\Targets\.start-local-cassandra.cmd

pushd ..\Assemblies\ElasticSearchVNext\Server\bin\
start elasticsearch.bat
popd

start "ServiceRunner" "..\Tools.Compiled\ServiceRunner\ServiceRunner.exe" "_StartAllConfigs\startAll.SR.yaml" "-startAllServices"