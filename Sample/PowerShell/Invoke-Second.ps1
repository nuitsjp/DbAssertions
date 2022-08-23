$server                 = 'localhost'
$database               = 'AdventureWorks'
$user                   = 'sa'
$password               = 'P@ssw0rd!'
$initializedDateTime    = '2022-08-21 11:30:33.807'
$output                 = '..\bin\Debug\Expected'

..\bin\Debug\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.exe second -s $server -d $database -u $user -p $password -i $initializedDateTime -o $output