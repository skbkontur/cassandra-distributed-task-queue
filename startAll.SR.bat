cd /d %~dp0

pushd ..\Assemblies\ElasticSearch\Server\bin\
start elasticsearch.bat
popd

pushd ..\Assemblies\Cassandra\Server\bin\
start cassandra.bat
popd

pushd Assemblies\Selenium\
start cmd /c chromedriver.exe 
popd

pushd Front
call npm install
start npm start
popd

%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe Deployment\LocalFire.xml /t:Fire /p:ServiceSuite=None;FrontEndSuite=DEFAULT

start "ServiceRunner" "..\Tools\ServiceRunner\ServiceRunner.exe" "_StartAllConfigs\startAll.SR.yaml"