$server     = 'localhost'
$database   = 'AdventureWorks'
$user       = 'sa'
$password   = 'P@ssw0rd!'
$output     = '..\bin\Debug\Expected'

..\bin\Debug\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.exe first -s $server -d $database -u $user -p $password -o $output