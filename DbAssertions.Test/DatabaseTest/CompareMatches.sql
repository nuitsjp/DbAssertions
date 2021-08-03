----------------------------------------------------------------
-- Compare
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
	FirstName = 'Compare'
where
	BusinessEntityID = 1

-- RunTime
update
	[AdventureWorks].[Person].[Person]
set
	ModifiedDate = '2020/01/02'
where
	BusinessEntityID = 1

-- Setup Time
update
	[AdventureWorks].[HumanResources].[Employee]
set
	ModifiedDate = '2019/12/31'
where
	BusinessEntityID = 1
