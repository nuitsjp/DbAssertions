docker-compose -f (Join-Path $PSScriptRoot 'AdventureWorks\start-database.yml') down
docker-compose -f (Join-Path $PSScriptRoot 'AdventureWorks\start-database.yml') up -d