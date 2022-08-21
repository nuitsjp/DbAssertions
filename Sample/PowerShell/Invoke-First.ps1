$server     = 'localhost'
$database   = 'AdventureWorks'
$user       = 'sa'
$password   = 'P@ssw0rd!'
$output     = '..\bin\Debug\Expected'

# first -s localhost -d AdventureWorks -u sa -p P@ssw0rd! -o Expected
..\bin\Debug\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.exe first -s $server -d $database -u $user -p $password -o $output