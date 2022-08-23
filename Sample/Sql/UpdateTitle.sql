use AdventureWorks

UPDATE
        Person.Person
SET
        TITLE = 'Mr.',
        ModifiedDate = getdate()
WHERE
        BusinessEntityID = 2

SELECT Person.ModifiedDate, * FROM Person.Person
