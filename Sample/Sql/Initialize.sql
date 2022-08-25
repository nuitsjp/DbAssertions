use AdventureWorks

UPDATE
	Person.Person
SET
	ModifiedDate = getdate()
WHERE
	BusinessEntityID = 1

SELECT Person.ModifiedDate, * FROM Person.Person
SELECT GETDATE() as 初期化完了時刻