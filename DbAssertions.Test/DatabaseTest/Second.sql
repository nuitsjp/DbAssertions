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
	ModifiedDate = '2019/12/01'
where
	BusinessEntityID = 1

-- Setup Time
update
	[AdventureWorks].[HumanResources].[Employee]
set
	ModifiedDate = '2019/12/01'
where
	BusinessEntityID = 1
