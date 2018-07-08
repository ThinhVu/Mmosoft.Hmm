USE master
CREATE DATABASE HmmTest
GO

USE HmmTest
GO

CREATE TABLE ProductTbl
(
	Id INT IDENTITY PRIMARY KEY,
	Name NVARCHAR(100) NOT NULL,
	Price INT,
	MFG DATETIME2,
	EXP DATETIME2
)
GO

CREATE Table Bill
(
	Id INT IDENTITY PRIMARY KEY,
	CreatedDate DATETIME
)
GO

CREATE TABLE BillDetail
(
	Id INT IDENTITY PRIMARY KEY,
	BillId INT NOT NULL,
	ProductId INT NOT NULL,
	Quantity INT NOT NULL
)
GO

-- SELECT * FROM Bill
-- SELECT * FROM BillDetail
-- SELECT * FROM ProductTbl
-- SELECT bo.CreatedDate as [Date], p.Id as [ProdId], p.Name as [ProdName], bo.Quantity as [Qty]
-- FROM ProductTbl as p 
-- JOIN (SELECT bi.Id, bi.CreatedDate, bd.ProductId, bd.Quantity
--      FROM Bill as bi
--       JOIN BillDetail as bd 
--       ON bi.Id = bd.BillId) as bo
-- ON bo.Id = 1 AND p.Id = bo.ProductId

-- TRUNCATE TABLE Bill
-- TRUNCATE TABLE ProductTbl
-- TRUNCATE TABLE BillDetail