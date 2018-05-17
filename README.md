## Overview

DataMapper is a fluent library used to map relational data to .NET objects. It uses ADO.NET underneath and can be used with any compliant ADO.NET provider (has been used successfully with SQL Server, Oracle, MySQL, DB2 Teradata and SQLite). The developer is in full control of the sql so all that is left is to map the result set to a list of objects. By default, the property name of the .NET object is mapped to the corresponding column name of the result set (using a case-insensitive match). The MapProperty method is used to define a mapping when the default will not suffice. Here is a simple example:
```
var users = new QueryBuilder<User>()
    .SetSql("select Id, FName, LName, EmailAddress from Users")
    .MapProperty(user => user.FirstName, "FName")
    .MapProperty(user => user.LastName, "LName")
    .GetResult()
    .List;
```
```users``` will be a ```List<User>``` with ```Id, FirstName, LastName and EmailAddress``` populated. If there are any additional fields in ```User```, they will have default values since there is no corresponding column in the result. You can, however, call MapProperty with a lamda function to populate a value. Let's say ```User``` has a ```FullName``` property. If we do nothing, it will be null (default value for string). But we could use the following code to populate the FullName:
```
var users = new QueryBuilder<User>()
    .SetSql("select Id, FName, LName, EmailAddress from Users")
    .MapProperty(user => user.FirstName, "FName")
    .MapProperty(user => user.LastName, "LName")
    .MapProperty(user => user.FullName, row => $"{row.GetString("LName")}, {row.GetString("FName")})
    .GetResult()
    .List;
```
Although you could compute ```FullName``` in the sql, it's cleaner to do it with .NET code since each sql dialect has it's own quirky syntax to do concatenation and you don't have to send extra data over the wire. This is just one simple example, but there are many more where .NET trumps SQL for simplicity and maintainability.

To avoid SQL injection attacks, be sure to use parameterized sql (note: using SQLite syntax. For SQL Server you would use ```@``` instead of ```$```). Here's an example:
```
var users = new QueryBuilder<User>()
    .SetSql("select Id, FName, LName, EmailAddress from Users where OrgId = $OrgId")
    .AddParameter("$OrgId", 5)
    .MapProperty(user => user.FirstName, "FName")
    .MapProperty(user => user.LastName, "LName")
    .MapProperty(user => user.FullName, row => $"{row.GetString("LName")}, {row.GetString("FName")})
    .GetResult()
    .List;
```
You can add as many parameters as you like and the order of calls to AddParameter and MapProperty does not matter.


