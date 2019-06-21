[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=tiagor87_idempotency&metric=alert_status)](https://sonarcloud.io/dashboard?id=tiagor87_idempotency)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=tiagor87_idempotency&metric=coverage)](https://sonarcloud.io/dashboard?id=tiagor87_idempotency)
[![NuGet](https://buildstats.info/nuget/Idempotency.Core)](http://www.nuget.org/packages/Idempotency.Core)
[![NuGet](https://buildstats.info/nuget/Idempotency.Redis)](http://www.nuget.org/packages/Idempotency.Redis)

# Idempotency

This packages focuses on predicting the flows needed to use idempotency.

## Idempotency.Core

Classes and interfaces required to use idempotency.

### What's in the package

* *Interface* __IIdempotencyKeyReader__

Interface responsible for identifying and reading the Request idempotence key.

* *Class* __IdempotencyKeyReader__

Class with default implementation to obtain idempotency key. It uses the endpoint address, method, and key used in the Request. This way, it does not allow duplication by address, method and key.

```csharp
public string Read(HttpRequest request)
{
  return $"[{request.Method}] {request.Path} - {request.Headers[IDEMPOTENCY_KEY_NAME]}";
}
```
* *Interface* __IIdempotencyRepository__

Interface responsible for save and retrieve key and value from some storage.

* *Class* __IdempotencyRegister__

Class relating to the registration of idempotency, it contains the key used, the status code returned in the operation and the response serialized JSON body.

* *Class* __IdempotencyMiddleware__

Middleware responsible for inserting idempotency logic into the request flows of WebAPI applications.

### How to install

* .Net Cli

```cmd
dotnet add package Idempotency.Core
```

* Package Manager

```cmd
Install-Package Idempotency.Core
```

### How to use

In the __Startup.cs__ file, add the following dependencies as scoped:

* __IIdempotencyKeyReader__ and *implementation*;
* __IIdempotencyRepository__ and *implementation*.

```csharp
services.AddScoped<IIdempotencyKeyReader, IdempotencyKeyReader>();
services.AddScoped<IIdempotencyRepository, IdempotencyRepository>();
```
And the middleware __IdempotencyMiddleware__.

```csharp
app.UseMiddleware<IdempotencyMiddleware>();
```

## Idempotency.Redis

Class and extensions to use *Redis* as registers storage.

### What's in the package

* *Class* __IdempotencyRepository__

__IIdempotencyRepository__ implementation that uses Redis as database.

* *Extensions* __Extensions__

Extensions to add Services and Middleware to WebAPI.

### How to install

* .Net Cli

```cmd
dotnet add package Idempotency.Redis
```

* Package Manager

```cmd
Install-Package Idempotency.Redis
```

### How to use

To add services:
```csharp
services.AddRedisIdempotency(Configuration);
```

To use middleware:
```csharp
app.UseIdempotency();
```
