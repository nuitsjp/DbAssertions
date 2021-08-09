----------------------------------------------------------------
-- First time
----------------------------------------------------------------
-- HostName
update
	[AdventureWorks].[Person].[Person]
set
	Suffix = '%HostName%'
where
	BusinessEntityID = 1

-- Random
update
	[AdventureWorks].[Person].[Person]
set
	FirstName = 'First'
where
	BusinessEntityID = 1

-- Run Time
update
	[AdventureWorks].[Person].[Person]
set
	ModifiedDate = '2000/1/1'
where
	BusinessEntityID = 1

-- Setup Time
update
	[AdventureWorks].[Person].[Person]
set
	ModifiedDate = '2000/1/3'
where
	BusinessEntityID = 2
