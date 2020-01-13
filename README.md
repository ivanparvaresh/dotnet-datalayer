dotnet-datalayer
============

A simple layer for accessing the database. Wrapping `EFCore` and `Mongodb` in a higher layer. 

# Quick Start
install the library by using to following command:
```bash
dotnet add package dotnet-datalayer --version 0.0.1
```

Create Datasource:
```csharp

// EFCore
using Dotnet.DataLayer.EntityFramework;
public class MyDatasource : EfCoreDatasource{
    public MyDatasource(DbContextOptions options) : base(options){}
}

// Mongo
using Dotnet.DataLayer.MongoDb;
public class MyDatasource : MongoDbDatasource{
    public MyDatasource(MongoDbDatasourceOptions options) : base(options){}
}
```

Create Session:
```csharp

// EFCore
using Dotnet.DataLayer.EntityFramework;
public class MySession : EfCoreSession<MyDatasource>{

    public DbSet<MyEntity> Tests {get; private set;}

    public MongoDbSession(MyDatasource datasource):base(datasource){}
}

// Mongo
using Dotnet.DataLayer.MongoDb;
public class MySession : MongoDbSession<MyDatasource>{

    public IMongoCollection<MyEntity> Tests {get; private set;}

    public MongoDbSession(MyDatasource datasource):base(datasource){}
}
```

Register as service:
```csharp

services.AddDatasource<MyDatasource,MySession>();

public class Client {

    public Client(MySession session){}
    
}

```

