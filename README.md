# THUInfo Web
The web server application of [THUInfo](https://github.com/thu-info-community/thu-info-app)
# Build Instruction
## Step 1
Install the .NET 10 SDK. The repository pins SDK `10.0.400` in `global.json` and permits rolling forward within the .NET 10 feature band.
If you are running RHEL or CentOS, just use
```
$ sudo dnf install dotnet-sdk-10.0
```
Others should follow this [installation instruction](https://learn.microsoft.com/en-us/dotnet/core/install/linux)
## Step 2
Clone this repo, and cd into folder ThuInfoWeb which contains the file ThuInfoWeb.csproj.
Then, run
```
dotnet build
```
and everything will be done by the .NET SDK CLI. Run `dotnet test` from the repository root to execute the test suite.
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

## APK Hosting

In production, mount a persistent directory at `/app/wwwroot/apk`. Copy the signed APK from the app release workflow into that directory without renaming it, for example `THUInfo_release_v3.16.4.apk`, and keep older releases available.

After the APK is present, use the admin page to check for the new Android version. The version check only publishes a release when its matching APK exists in the mounted directory.
