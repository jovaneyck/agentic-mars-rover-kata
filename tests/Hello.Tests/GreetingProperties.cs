using FsCheck;
using FsCheck.Xunit;
using Hello.Core;

namespace Hello.Tests;

public class GreetingProperties
{
    [Property]
    public bool Greeting_wraps_any_name(NonNull<string> name) =>
        Greeting.For(name.Get) == "Hello, " + name.Get + "!";
}
