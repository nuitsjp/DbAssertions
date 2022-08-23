use AdventureWorks

UPDATE
	Person.Person
SET
	ModifiedDate = getdate()
WHERE
	BusinessEntityID = 1

SELECT Person.ModifiedDate, * FROM Person.Person
SELECT GETDATE() as ‰Šú‰»Š®—¹