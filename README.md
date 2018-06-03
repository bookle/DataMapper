# Overview

DataMapper is a fluent library used to map relational data to .NET objects. It uses ADO.NET underneath and can be used with any compliant ADO.NET provider (has been used successfully with SQL Server, Oracle, MySQL, DB2 Teradata and SQLite). The developer is in full control of the sql so all that is left is to map the result set to a list of objects. By default, the property name of the .NET object is mapped to the corresponding column name of the result set (using a case-insensitive match). The MapProperty method is used to define a mapping when the default will not suffice.

## Usage

Note: using ```new QueryBuilder``` for brevity/demonstation purposes. In reality, you would derive a concrete builder from QueryBuilder.

### Simple mapping

```csharp

var users = new QueryBuilder<User>()
    .SetSql("select Id, FName, LName, EmailAddress from Users")
    .MapProperty(user => user.FirstName, "FName")
    .MapProperty(user => user.LastName, "LName")
    .GetResult()
    .List;

```

Another option would be to just alias the columns in sql

```csharp

var users = new QueryBuilder<User>()
    .SetSql("select Id, FName As FirstName, LName As LastName, EmailAddress from Users")
    .GetResult()
    .List;

```

The MapProperty calls do have advantages such as compile-time checking and times when aliasing the columns is just not an option - e.g., a stored procedure that's used by other clients.

```users``` will be a ```List<User>``` with ```Id, FirstName, LastName and EmailAddress``` populated. If there are any additional fields in ```User```, they will have default values since there is no corresponding column in the result. 

### Use a function to map value

You can, however, call MapProperty with a lamda function to populate a value. Let's say ```User``` has a ```FullName``` property. If we do nothing, it will be null (default value for string). But we could use the following code to populate the FullName:

```csharp

var users = new QueryBuilder<User>()
    .SetSql("select Id, FName, LName, EmailAddress from Users")
    .MapProperty(user => user.FirstName, "FName")
    .MapProperty(user => user.LastName, "LName")
    .MapProperty(user => user.FullName, row => $"{row.GetString("LName")}, {row.GetString("FName")})
    .GetResult()
    .List;

```

Although you could compute ```FullName``` in the sql, it's usually better to do it with .NET code since each sql dialect has different syntax to do concatenation and you don't have to send extra data over the wire. This is just one simple example, but there are many more where .NET code beats SQL for simplicity and maintainability.

### Map Enum values

Another feature is automatically mapping integer values to Enum values, eliminating repetitive, boilerplate conversion code. Let's say ```User``` has a ```Role``` property with ```Role``` being an enum defined as follows:

```csharp

    public enum Role
    {
        Admin = 1,
        Manager = 7,
        Worker = 10
    }

```

Then, we could just do this without any expicit conversion code:

```csharp

var users = new QueryBuilder<User>()
    .SetSql("select Id, FName, LName, EmailAddress, Role from Users")
    .MapProperty(user => user.FirstName, "FName")
    .MapProperty(user => user.LastName, "LName")
    .MapProperty(user => user.FullName, row => $"{row.GetString("LName")}, {row.GetString("FName")})
    .GetResult()
    .List;

```

For this to work, enum values must be integers and the db type should be convertible to integer (byte, smallint, int).

### Natural Mappings

Data values should map naturally to .NET values. If a value can be null, you should always map it to a nullable type. Blob types should map to byte array. Unique identifiers should map to Guid.

### Named Parameters

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

Here are some code examples using the following SQLite table definition, SQL Server stored procedure and .NET class files.

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

```sql

Create Procedure GetCustomersByZip
(
  @zip varchar (11),
  @count Integer OUTPUT
)
AS

select
 CustomerId,
 FirstName,
 LastName,
 Company,
 Address,
 City,
 State,
 Country,
 PostalCode
from
 Customers
where
 PostalCode = @zip
set @count = @@Rowcount

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

### Concrete SQLite Query Builder

```csharp

class SQLiteQueryBuilder<T> : QueryBuilder<SQLiteConnection, T> where T : class, new()
{
    public SQLiteQueryBuilder()
    {
        this.SetConnectionString(@"Data Source=Data\chinook.db;Version=3;");
    }
}

```

### Concrete SQL Server Query Builder

```csharp

class SqlQueryBuilder<T> : QueryBuilder<SqlConnection, T> where T : class, new()
{
    public SqlQueryBuilder()
    {
        this.SetConnectionString(@"Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;");
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

### Call a stored procedure

```csharp

var customerResult = new SqlQueryBuilder<Customer>()
    .SetStoredProcedure("GetCustomersByZip")
    .AddParameter("@zip", "30004")
    .AddParameter("@count", -1, QueryParameterDirectionEnum.Output)
    .MapProperty(c => c.Zip, "PostalCode")
    .MapProperty(c => c.FullName, row => $"{row.GetString("FirstName")} {row.GetString("LastName")}")
    .MapProperty(c => c.DateAdded, row => ConvertToDate(row.GetString("CreateDate")))
    .GetResult();

var customers = customerResult.List;
var customerCount = Convert.ToInt32(customerResult.Parameters.First(p => p.Name == "@count").Value);

```





