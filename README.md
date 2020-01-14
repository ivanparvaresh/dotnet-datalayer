dotnet-datalayer
============

A simple layer for accessing the database. Wrapping `EFCore` and `Mongodb` in a higher layer. 

# Quick Start
install the library by using to following command:
```bash
dotnet add package dotnet-datalayer --version 0.0.1
```

Create DataContext:
```csharp

// EFCore
using Dotnet.DataLayer.EntityFramework;
public class MyDataContext : DatabaseContext{
    public MyDatasource(DbContextOptions options) : base(options){}
}

// Mongo
using Dotnet.DataLayer.MongoDb;
public class MyDataContext : DatabaseContext{
    public MyDatasource(DbContextOptions options) : base(options){}
}
```

Register as service:
```csharp

services.AddDatabaseContext<MyDataContext>();

public class Client {

    public Client(MyDataContext session){
        
    }
    
}

```

