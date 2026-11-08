using Hello.Core;

namespace Hello.Tests;

public class GreeterTests
{
    private sealed class FixedNameProvider(string name) : INameProvider
    {
        public string GetName() => name;
    }

    private sealed class InMemoryGreetingSink : IGreetingSink
    {
        public List<string> Written { get; } = [];
        public void Write(string greeting) => Written.Add(greeting);
    }

    [Fact]
    public void Greets_the_provided_name()
    {
        var sink = new InMemoryGreetingSink();

        new Greeter(new FixedNameProvider("Jo"), sink).Greet();

        Assert.Equal(["Hello, Jo!"], sink.Written);
    }
}
