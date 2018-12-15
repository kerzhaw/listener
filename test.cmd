dotnet clean src/listener
dotnet clean test/listener.Tests
dotnet build src/listener
dotnet build test/listener.Tests
dotnet test test\listener.Tests