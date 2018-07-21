# Overview

DataMapper is a fluent library used to map relational data to .NET objects. It uses ADO.NET underneath and can be used with any compliant ADO.NET provider (has been used successfully with SQL Server, Oracle, MySQL, DB2, Teradata and SQLite). The developer is in full control of the sql so all that is left is to map the result set to a list of objects. By default, the property name of the .NET object is mapped to the corresponding column name of the result set (using a case-insensitive match). The MapProperty method is used to define a mapping when the default will not suffice. 

## Limitations

This library is not a full-fledged ORM, nor does it claim to be. For intance, you cannot (at the moment) use this to lazy load child collections or eagerly load child collections in one fell swoop as is done, with an enormous payload, in other mainstream ORM's. No, this library, can be used to perform simple one-level deep mappings so you would need to design your calls to query data when needed and not have requirements to return the entire object graph in one call.

## Usage

Note: using ```new QueryBuilder``` for brevity/demonstration purposes. In reality, you would derive a concrete builder from QueryBuilder.

### Simple mapping

```csharp

class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
}

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

enum Role
{
    Admin = 1,
    Manager = 7,
    Worker = 10
}

```

Then, we could just do this without any expicit conversion code:

```csharp

class User
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }
    public Role Role { get; set; }
}

var users = new QueryBuilder<User>()
    .SetSql("select Id, FName, LName, EmailAddress, RoleId from Users")
    .MapProperty(user => user.FirstName, "FName")
    .MapProperty(user => user.LastName, "LName")
    .MapProperty(user => user.FullName, row => $"{row.GetString("LName")}, {row.GetString("FName")})
    .MapProperty(user => user.Role, "RoleId")
    .GetResult()
    .List;

```

For this to work, enum values must be integers and the db type should be convertible to integer (byte, smallint, int).

### Natural Mappings

Data values should map naturally to .NET values. If a field (or column) can be null, you should always map it to a nullable type. Blob types (or varbinary) should map to byte array. Unique identifiers should map to Guid. Dates and DateTimes should map to ... you get the idea.

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

In this example, I show how to use the QueryResult return value to get not only the list but also the parameters where we can return output values. The parameters would also be handy in unit testing that correct parameters (input and output) are defined.

## Unit Testing

How in the f*** do I mock this thing so I can cover my client code with warm and fuzzy test code?

### Here's one overly simple way - subclass your concrete QueryBuilder

Let's say you have a ```SqlQueryBuilder``` that derives from ```QueryBuilder```. You can then derive a new class from ```SqlQueryBuilder``` called ```MockSqlQueryBuilder```.
All you need to do is override the GetResult method to return whatever you like.

### Here's another more complicated and versatile way

QueryBuilder has a constructor that takes 1 function and 1 action: ```createCommand``` and ```openConnection```. The ```createCommand``` function returns an instance of ```IDbCommand``` and ```openConnection``` is just an action so you can mock this to do nothing. You need to mock ```IDbCommand``` and specifically the methods, ```CreateParameter``` and/or ```ExecuteReader``` and/or ```ExecuteScalar``` and/or ```ExecuteNonQuery```. If you mock ```ExecuteReader```, then ```MockCommand``` should have a constructor that takes a ```DataTable```. Using the ```DataTable```, you can create a ```DataReader``` that you can return from ```ExecuteReader```. This ```DataReader``` will be used to produce the list when calling the actual ```QueryBuilder.GetResult```.

### DataTableBuilder

In the test project, there is a DataTableBuilder. This is a utility that can be used to create a DataTable. The idea is to create a .NET class with properties that map to the query you are using.

Let's say you have a repository with a dependency on QueryBuilder.

You have been a good boy and propery injected your QueryBuilder into your repository class.
In one of your methods, you make this call:

```csharp

var sql = "select Id, FirstName, LastName, CreateDate from Users";
var users = QueryBuilder<User>()
    .SetSql(sql)
    .GetResult()
    .List;

return users;

```

You need to mock the QueryBuilder and inject the mock into your repository instance. But first, you need a DataTable. To create that, you can just manually create a DataTable and define all your columns with the correct types and nullability, etc. or you can create a test class that maps to the type of data your query will return and use the DataTableBuilder to create the DataTable for you.

```csharp

class TestUser
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime CreateDate { get; set; }
}

```

Using this class, create a list of users:

```csharp

var users = new List<TestUser> ...

```

Then, you can use this list to create a DataTable using DataTableBuilder.

```csharp

var dt = new DataTableBuilder<TestUser>(users).Build();
var command = new MockCommand(dt);
// create an instance of the real QueryBuilder, not a mock, and pass it the createCommand and openConnection.
var sqlBuilder = new SqlQueryBuilder<User>(() => command, conn => { });
var repository = new UserRepository(sqlBuilder);
var users = repository.GetUsers();

```

From there, you can make assertions on the data you get back.






