Param(
    [Parameter(Mandatory)]
    [string] $Version
)

dotnet publish .\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.csproj -c Release -r win10-x64 -f net6.0 /p:Version=$Version --self-contained=false
dotnet publish .\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.csproj -c Release -r win10-x64 -f net48 /p:Version=$Version