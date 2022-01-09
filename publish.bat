rmdir /s /q Publish

dotnet clean Munin.Node.sln -c Release -nodeReuse:false
dotnet build Munin.Node.sln -c Release -nodeReuse:false

dotnet publish Munin.Node.Plugins.Hardware\Munin.Node.Plugins.Hardware.csproj -o Publish -c Release
dotnet publish Munin.Node.Plugins.PerformanceCounter\Munin.Node.Plugins.PerformanceCounter.csproj -o Publish -c Release
dotnet publish Munin.Node.Plugins.SensorOmron\Munin.Node.Plugins.SensorOmron.csproj -o Publish -c Release
dotnet publish Munin.Node.Service\Munin.Node.Service.csproj -o Publish -c Release
