if not OBJECT_ID('dbo.MatrixFunction') is null
	drop function dbo.MatrixFunction

GO
-- Testing function parsing
Create Function dbo.MatrixFunction (@a as int = 0, @b as int = 0)
returns @return table
(
	p1 float, p2 float, p3 float
)
AS
begin
declare @i int = 0
declare @x Table(id int primary key identity(1,1), num float null)
declare @y table(id int primary key identity(1,1), num float null)

WHILE @i < 3 begin
	insert into @x (num) select @a + @i
	set @i = @i + 1
end

set @i = 0
WHILE @i < 3 begin
	insert into @y (num) select @b + @i 
	set @i = @i + 1
end
insert into @return(p1, p2, p3) 
select top 3 x.x1 * y.num, x.x2 * y.num, x.x3 * y.num
	from (select (select num from @x where id = 1) x1, (select num from @x where id = 2) x2, (select num from @x where id = 3) x3 ) x,
		@y y
return 
end
