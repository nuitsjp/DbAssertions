Param(
    [Parameter(Mandatory)]
    [string] $Version
)

dotnet publish .\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.csproj -c Release -r win-x64 -f net8.0 /p:Version=$Version --self-contained=false
dotnet publish .\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.csproj -c Release -r win-x64 -f net48 /p:Version=$Version