if not OBJECT_ID('dbo.CreateMatrixFromXml') is null
	DROP FUNCTION dbo.CreateMatrixFromXml

GO

CREATE FUNCTION dbo.CreateMatrixFromXml (@xml xml)
returns TABLE
as
return select t.r.value('p1[1]','float') p1, t.r.value('p2[1]','float') p2, t.r.value('p3[1]','float') p3 
	from @xml.nodes('/matrix/row') as t(r)

