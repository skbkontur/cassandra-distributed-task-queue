C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -i

pushd ..\Assemblies\ElasticSearch\Server\bin\
start elasticsearch.bat
popd

cd ..\Assemblies\Cassandra\Server\bin\
start cassandra.bat

pushd Front
call npm install
start npm start
popd

cd ..\..\..\..\RemoteTaskQueue\

if exist ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe (start cmd /c ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe)
ping 123.45.67.89 -n 1 -w 1000 > nul
if exist ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe (start cmd /c ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe)
ping 123.45.67.89 -n 1 -w 1000 > nul
if exist ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe (start cmd /c ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe)
ping 123.45.67.89 -n 1 -w 1000 > nul
if exist ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe (start cmd /c ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe)
ping 123.45.67.89 -n 1 -w 1000 > nul
if exist ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe (start cmd /c ExchangeService\bin\Debug\Catalogue.RemoteTaskQueue.ExchangeService.exe)

start cmd /c MonitoringService\bin\Debug\Catalogue.RemoteTaskQueue.MonitoringService.exe
start cmd /c TaskCounter.TestService\bin\Debug\Catalogue.RemoteTaskQueue.TaskCounter.TestService.exe
start cmd /c ElasticMonitoring.TestService\bin\Debug\Catalogue.RemoteTaskQueue.ElasticMonitoring.TestService.exe

%WinDir%\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe Deployment\LocalFire.xml /t:Fire /p:ServiceSuite=None;FrontEndSuite=DEFAULT

cd ..

pushd Assemblies\Selenium\
start cmd /c chromedriver.exe 
popd