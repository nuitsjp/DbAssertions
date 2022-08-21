$server                 = 'localhost'
$database               = 'AdventureWorks'
$user                   = 'sa'
$password               = 'P@ssw0rd!'
$initializedDateTime    = '2022-08-21 11:30:33.807'
$output                 = '..\bin\Debug\Expected'

# second -s localhost -d AdventureWorks -u sa -p P@ssw0rd! -o Expected -i "2022-08-20 22:53:28.623"
..\bin\Debug\DbAssertions.SqlServer.App\DbAssertions.SqlServer.App.exe second -s $server -d $database -u $user -p $password -i $initializedDateTime -o $output