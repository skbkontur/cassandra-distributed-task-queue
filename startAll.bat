C:\Windows\Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe -i

cd ..\Assemblies\Cassandra\Server\bin\
start cassandra.bat

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

start cmd /c IndexService\bin\Debug\Catalogue.RemoteTaskQueue.IndexService.exe

start cmd /c MonitoringService\bin\Debug\Catalogue.RemoteTaskQueue.MonitoringService.exe

cd ..

pushd Assemblies\Selenium\
start cmd /c chromedriver.exe 
popd

C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe RemoteTaskQueue\DeployFronts.xml