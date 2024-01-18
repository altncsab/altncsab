# SQL Script runner

This is a C# Windows forms application tool using 4.8.1 .net Framework.

It could run under Linux too if Mono or Vine is implemented with the right framework.

## Releases

1.0.0.0 - Tool is ready to use, Users Guide is finished.

## Purpose

Demonstrate SQL server knowledge and dealing with SQL Scripts

This tool created to apply SQL script files to SQL Server database. The problem appear when the required script is not in orders or different dependent part of the code located in different files or in the same file but not in the proper order.

This tool is to analyze the execution of scripts and find the proper execution order: Types, Functions, Procedures, Tables, etc.

See usage is in [Users Guide](./UsersGuide/UsersGuide.md)

Sample project Script is in folder SampleProject.
The file "TableCreationScript.sql" script will fail to execute due to a missing type definition.
The file "MatrixType.sql" will fail to execute if there is any reference already to this type in the database.

This tool do not give solution to resolve this problems, but later it is planned to have new feature to auto resolve some typical problems during database implementation.

## Planned features in the future

- Enable / Disable Transaction
- Auto script drop statement if user asking for it.
- Comment analysis result to the header of generated script (Like missing object or risk of failure.)
- Checkbox to include / exclude script section in Execution monitor
- Option to mark script item "to skip" if object already exists in Database
- Option to allow inject drop statement if object already exists.
- Maybe a procedure for type alteration problem, when a type in use but it must be changed. Those referenced objects needs to be scripted, dropped and recreated with the new type definition!

Email me if you have any other idea for make the tool better for your purpose.
