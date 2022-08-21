$project = '..\..\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.csproj'
$output = '..\bin\Debug\DbAssertions.SqlServer.App'

dotnet publish $project /t:Rebuild -c Release -f net6.0 -o $output -r win10-x64 --self-contained=false