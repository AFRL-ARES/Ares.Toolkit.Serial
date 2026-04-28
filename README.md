# Ares.Toolkit.Serial

Simple .NET serial communication toolkit used in ARES to streamline serial device development.

Can also be used as a general "programmer friendly" way of making serial device integrations. 


## Features

- Hardware serial connection support (`AresHardwareConnection`)
- Fluent connection builder (`SerialConnectionBuilder`)
- Simple line-based command/response helper (`SendLine`)
- Raw command helpers (`SendRaw`, `SendString`)
- Simulation base class for tests (`AresSerialSimConnection`)
- Custom command/response creation (`SerialCommand`, `SerialCommandWithResponse`, `SerialResponse`)

## Install

```bash
dotnet add package Ares.Toolkit.Serial
```

## Quick Start

```csharp
using Ares.Toolkit.Serial;

var connection = new SerialConnectionBuilder()
    .WithPort("COM3")
    .WithBaudRate(115200)
    .WithTimeout(TimeSpan.FromSeconds(5))
    .Build();

connection.AttemptOpen();

var response = await connection.SendLine("PING");
Console.WriteLine(response);

connection.Close();
```

You can also create new custom commands by extending the abstract classes such as SerialCommand and SerialCommandWithResponse. That way you could specify strongly typed contracts.
```csharp
class MyCommand : SerialCommandWithResponse<MyResponse> 
{
    ...
}

MyResponse response = await connection.Send(new MyCommand());
```

## Target Framework

- `net10.0`

## Development

From the `Ares.Toolkit.Serial` directory:

```bash
dotnet build
dotnet test
```

## CLEARANCE
Distribution A. Approved for public release: distribution unlimited. AFRL-2025-5329
