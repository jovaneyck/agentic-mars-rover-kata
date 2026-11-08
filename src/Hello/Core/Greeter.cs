namespace Hello.Core;

public interface INameProvider
{
    string GetName();
}

public interface IGreetingSink
{
    void Write(string greeting);
}

public static class Greeting
{
    public static string For(string name) => $"Hello, {name}!";
}

public sealed class Greeter(INameProvider names, IGreetingSink sink)
{
    public void Greet() => sink.Write(Greeting.For(names.GetName()));
}
