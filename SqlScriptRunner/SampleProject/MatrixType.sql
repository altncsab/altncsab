IF not TYPE_ID('dbo.MatrixType') IS NULL  
	DROP TYPE dbo.MatrixType
GO
-- Sample type for Martix example
Create Type dbo.MatrixType as Table(p1 float, p2 float, p3 float)
