using Hello.Adapters;
using Hello.Core;

new Greeter(new ArgsNameProvider(args), new ConsoleGreetingSink()).Greet();
