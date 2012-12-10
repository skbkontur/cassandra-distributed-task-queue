C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe RemoteTaskQueue.sln /target:Clean;Rebuild /verbosity:m /p:zip=false	

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

start cmd /c MonitoringService\bin\Debug\Catalogue.RemoteTaskQueue.MonitoringService.exe