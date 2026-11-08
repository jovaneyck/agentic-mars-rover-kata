using Hello.Core;

namespace Hello.Adapters;

public sealed class ArgsNameProvider(string[] args) : INameProvider
{
    public string GetName() => args.FirstOrDefault() ?? "World";
}

public sealed class ConsoleGreetingSink : IGreetingSink
{
    public void Write(string greeting) => Console.WriteLine(greeting);
}
