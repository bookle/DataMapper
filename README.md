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

```csharp

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

## Examples

Here are some code examples using the following SQLite table definition and .NET class files.

```sql

CREATE TABLE `Customer` (
    `CustomerId`    INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    `FirstName` NVARCHAR ( 40 ) NOT NULL,
    `LastName`  NVARCHAR ( 20 ) NOT NULL,
    `Company`   NVARCHAR ( 80 ),
    `Address`   NVARCHAR ( 70 ),
    `City`  NVARCHAR ( 40 ),
    `State` NVARCHAR ( 40 ),
    `Country`   NVARCHAR ( 40 ),
    `PostalCode`    NVARCHAR ( 10 ),
    `Phone` NVARCHAR ( 24 ),
    `Fax`   NVARCHAR ( 24 ),
    `Email` NVARCHAR ( 60 ) NOT NULL,
    `SupportRepId`  INTEGER,
    `CreateDate` NVARCHAR (30) NOT NULL,
    FOREIGN KEY(`SupportRepId`) REFERENCES `Employee`(`EmployeeId`) ON DELETE NO ACTION ON UPDATE NO ACTION
);

```

```csharp

class Customer
{
    public int CustomerId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public string Phone { get; set; }
    public string EmailAddress { get; set; }
    public DateTime? DateAdded { get; set; }
}

```

```csharp

class SQLiteQueryBuilder<T> : QueryBuilder<SQLiteConnection, T> where T : class, new()
{
    public SQLiteQueryBuilder()
    {
        this.SetConnectionString(@"Data Source=Data\chinook.db;Version=3;");
    }
}

```

### Populate a list of customers

```csharp

var customers = new SQLiteQueryBuilder<Customer>()
    .SetSql("select * from Customer")
    .GetResult().List;

```

(Note: Zip, FullName and DateAdded will be null)

### Populate a list of customers with all fields populated

```csharp

var customers = new SQLiteQueryBuilder<Customer>()
    .SetSql("select * from Customer where CustomerId = $CustomerId")
    .AddParameter("$CustomerId", 19)
    .MapProperty(c => c.Zip, "PostalCode")
    .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
    .MapProperty(c => c.DateAdded, row => ConvertToDate(row.GetString("CreateDate")))
    .GetResult().List;

```


