# THUInfo Web
The web server application of [THUInfo](https://github.com/UNIDY2002/THUInfo)
# Build Instruction
## Step 1
Install dotnet sdk 8.0.
If you are running RHEL or CentOS, just use
```
$ sudo dnf install dotnet-sdk-8.0
```
Others should follow this [installation instruction](https://docs.microsoft.com/zh-cn/dotnet/core/install/linux)
## Step 2
Clone this repo, and cd into folder ThuInfoWeb which contains the file ThuInfoWeb.csproj.
Then, run
```
dotnet build
```
and everything will be done by the .net sdk cli.
## Step 3
Set up configuration.
Open appsettings.json, input your postgresql connection string into "Test" node.
Then uncomment "Kestrel" node and complete the settings.
## Step 4
Finally, run
```
dotnet run
```
and the application should be started.