if not OBJECT_ID('dbo.CreateXmlFromMatrix') is null
	drop function dbo.CreateXmlFromMatrix
GO

Create Function dbo.CreateXmlFromMatrix(@matrix as dbo.MatrixType readonly)
returns xml
as begin
declare @xml as xml
set @xml = (select cast(p1 as float) p1 , cast(p2 as float) p2, cast(p3 as float) p3 from @matrix for xml path('row'), root('matrix'))
return @xml
end
