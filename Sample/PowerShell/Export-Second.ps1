Param (
    [Parameter(Mandatory)]
    $InitializedDateTime
)
$server                 = 'localhost'
$database               = 'AdventureWorks'
$user                   = 'sa'
$password               = 'P@ssw0rd!'
$output                 = '..\bin\Debug\net6.0\Expected'

..\bin\Debug\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.exe second -s $server -d $database -u $user -p $password -i $InitializedDateTime -o $output