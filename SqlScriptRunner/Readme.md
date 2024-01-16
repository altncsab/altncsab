# SQL Script runner

This is a C# Windows forms application tool using 4.8.1 .net Framework.

## Releases

1.0.0.0 - Tool is ready to use.

## Purpose

Demonstrate SQL server knowledge and dealing with SQL Scripts

This tool created to apply SQL script files to SQL Server database. The problem appear when the required script is not in orders or different dependent part of the code located in different files or in the same file but not in the proper order.

This tool is to analyze the execution of scripts and find the proper execution order: Types, Functions, Procedures, Tables, etc.

See usage is in [Users Guide](./UsersGuide/UsersGuide.md)

Sample project Script is in folder SampleProject.
The file "TableCreationScript.sql" script will fail to execute due to a missing type definition.
The file "MatrixType.sql" will fail to execute if there is any reference already to this type in the database.

This tool do not give solution to resolve this problems, but later it is planned to have new feature to auto resolve some typical problems during database implementation.
