----------------------------------------------------------------
-- Second time
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
	FirstName = 'Second'
where
	BusinessEntityID = 1

-- RunTime
update
	[AdventureWorks].[Person].[Person]
set
	ModifiedDate = '2000/2/1'
where
	BusinessEntityID = 1

-- Setup Time
update
	[AdventureWorks].[Person].[Person]
set
	ModifiedDate = '2000/2/3'
where
	BusinessEntityID = 2
