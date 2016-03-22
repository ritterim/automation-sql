# Automation : SQL

This library is designed to allow the creation of **LocalDb** databases via C#. Mainly used to write integration tests, you get a fully functional SQL database that has full SQL Express capabilities.

![NuGet Version](https://img.shields.io/nuget/v/RimDev.Automation.Sql.svg)
![NuGet Download Count](https://img.shields.io/nuget/dt/RimDev.Automation.Sql.svg)

## Prerequisites

To use this library, you will need SQL Express 2012 or SQL Express 2014 versions of LocalDb installed. We recommend you installing both via [Chocolatey](https://chocolatey.org/).

## Quick Start

Install via:

[`Install-Package RimDev.Automation.Sql`](https://www.nuget.org/packages/RimDev.Automation.Sql/)

The **LocalDb** class will perform the following tasks:

1. Create the database files in the path specified
2. Connect the files to your Localdb instance
3. Return a connection string that can be utilized with any SQL Capable tool (ORM, Mini-ORM, ADO.NET)

To use the class, do the following in your test.

```csharp
using (var database = new LocalDb()) {
  // your code goes here
}

```

A simple use case utilizing ADO.NET would look something like this.

```csharp
using (var database = new LocalDb())
{
  using (var connection = database.OpenConnection())
  {
    var command =
       new SqlCommand("CREATE TABLE Customers (" +
          "DrvLicNbr NVarChar(50), " +
          "DateIssued Date," +
          "DateExpired Date," +
          "FullName nvarchar(120)," +
          "Address NVARCHAR(120)," +
          "City nvarchar(50)," +
          "State nvarchar(100)," +
          "PostalCode nvarchar(20)," +
          "HomePhone nvarchar(20)," +
          "OrganDonor bit);",
          connection);

        // created a new table
        command.ExecuteNonQuery();
  }
}
```

## Configuration

The LocalDB class is initialized completely via the constructor. There are *four* options you may set. All are optional.

1. **Database name** : The name of the database as seen in SQL Server Management studio. If not specified the naming will be **localdb_<DateTime.Not.Ticks>**.
2. **Version** : There are two versions supported for LocalDB, which are **v11** and **v12**. By default we try to use v11 as it is most likely installed.
3. **Location** : The location where the database files will be created (log and mdf). By default it will be in **Assembly.GetExecutingAssembly().Location**.
4. **Database prefix** : This is used as the prefix when creating the database name, if the database name is not specified. By default this value is *"localdb"*.

Notice each are optional and there are safe rational defaults.

## Gotcha's

### LocalDB installed but can't Connect

You may have LocalDB installed, but never initialized the instance on your machine. Run this command via command prompt.

LocalDB SQL EXPRESS 2014

> "C:\Program Files\Microsoft SQL Server\120\Tools\Binn\SqlLocalDB.exe" create "v12.0" 12.0 -s

LocalDB SQL Express 2012

> "C:\Program Files\Microsoft SQL Server\110\Tools\Binn\SqlLocalDB.exe" create "v11.0" 11.0 -s

Verify that the command worked by using **SQL Server Management Studio** to connect to the instance.

## Contributors

- Khalid Abuhakmeh (@buhakmeh)
- Justin Rusbatch (@jrusbatch)

## Thanks

Thanks to [Ritter IM](http://ritterim.com) for supporting OSS.
