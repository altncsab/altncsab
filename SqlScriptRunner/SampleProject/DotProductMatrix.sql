IF not OBJECT_ID('dbo.DotProductMatrix') is null
	drop function dbo.DotProductMatrix

GO
Create Function dbo.DotProductMatrix (@a dbo.MatrixType readonly, @b as dbo.MatrixType readonly)
returns @return table
(
	p1 float, p2 float, p3 float
)
AS
begin
	declare @a1 Table(id int identity(1,1) primary key, p1 float, p2 float, p3 float)
	insert into @a1 (p1,p2,p3) select * from @a
	declare @b1 Table(id int identity(1,1) primary key, p1 float, p2 float, p3 float)
	insert into @b1 (p1,p2,p3) select * from @b
	declare @i int = 1
	declare @j int = 1
	declare @p1 float, @p2 float, @p3 float 
	declare @q1 float, @q2 float, @q3 float 
	declare @r float, @r1 float, @r2 float, @r3 float 
	while @i <= 3 begin
		set @p1 = 0 set @p2 = 0 set @p3 = 0
		select @p1 = p1, @p2 = p2, @p3 = p3 from @a1 where id = @i
		while @j <= 3 begin
			set @q1 = 0 set @q2 = 0 set @q3 = 0
			select @q1 = case when @j = 1 then p1
				else case when @j = 2 then p2  
					else case when @j = 3 then p3 else 0 end
					end
				end
			from @b1 where id = 1
			select @q2 = case when @j = 1 then p1
				else case when @j = 2 then p2  
					else case when @j = 3 then p3 else 0 end
					end
				end
			from @b1 where id = 2
			select @q3 = case when @j = 1 then p1
				else case when @j = 2 then p2  
					else case when @j = 3 then p3 else 0 end
					end
				end
			from @b1 where id = 3
			set @r = @q1 * @p1 + @q2 * @p2 + @q3 * @p3
			if @j = 1 set @r1 = @r
			if @j = 2 set @r2 = @r
			if @j = 3 set @r3 = @r
			set @j = @j + 1
		end
		insert into @return (p1, p2, p3)
			select @r1, @r2, @r3
		set @i = @i + 1
		set @j = 1
	end
return
end