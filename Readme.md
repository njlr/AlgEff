# AlgEff - Algebraic Effects for F#
## What are algebraic effects?
Algebraic effects provide a way to define and handle side-effects in  functional programming. This approach has several important benefits:
* Effects are defined in a purely functional way. This eliminates the danger of unexpected side-effects in otherwise pure functional code.
* Implementation of effects (via "handlers") is separate from the effects' definitions. You can handle a given effect type multiple different ways, depending on your needs. (For example, you can use one handler for unit tests, and another for production.)

In summary, you can think of algebraic effects as functional programming's answer to dependency injection in object-oriented programming. They solve a similar problem, but in a more functional way.
## Why use AlgEff?
AlgEff is one of the few algebraic effect systems for F#. It was inspired by a similar F# library called [Eff](https://github.com/palladin/Eff) and by [Scala's ZIO](https://zio.dev/). Reasons to use AlgEff:
* Effects are easy to define.
* Handlers are easy to define.
* Programs that use effects and handlers are easy to write.
* Strong typing reduces the possibility of unhandled effects.
## Writing an effectful program
Let's write a simple effectful program that interacts with the user via a console and then logs the result:
```fsharp
let program =
    effect {
        do! Console.writeln "What is your name?"
        let! name = Console.readln
        do! Console.writelnf "Hello %s" name
        do! Log.writef "Name is %s" name
        return name
    }
```
The type of this value is:
```fsharp
Program<'ctx, string when 'ctx :> LogContext and 'ctx :> ConsoleContext>
```
The first type parameter (`'ctx`) indicates that the program requires handlers for both logging and console effects, and the second one (`string`) indicates that the program returns a string. It's important to understand that this program doesn't actually **do** anything until it's executed. The `program` value itself is purely functional -- no side-effects occurred while creating it.
## Creating a runtime environment
In order to run this program (and potentially cause actual side-effects), we must define an environment that satisfies the program's requirements:
```fsharp
type ProgramEnv<'ret>() as this =
    inherit Environment<'ret>()

    let handler =
        Handler.combine2
            (PureLogHandler(this))
            (ActualConsoleHandler(this))
    
    interface ConsoleContext
    interface LogContext

    member __.Handler = handler
```
The important thing to note here is that our environment contains both a log handler (`PureLogHandler`) and a console handler (`ActualConsoleHandler`). In this case, we've decided to use a log handler that is purely functional (it doesn't perform any I/O) and a console handler that invokes a real command-line console. We could easily have made other choices.
## Running an effectful program
Now that we have both a program and an environment that satisfies its requirements, we can actually run it:
```fsharp
let name, (log, Unit) =
    ProgramEnv().Handler.Run(program)
```
Running a program returns a 2-tuple where the first element is the value returned by the program (`name`) and the second element is the final state of the environment's handlers. Because there are two handlers, the final state is itself a 2-tuple containing the log (`log`) and the console state (`Unit` here because we used an actual console with side-effects rather than simulating I/O in memory). The resulting console might look like this:
```
What is your name?
Kristin
Hello Kristin
```
And the corresponding log would contain a single entry:
```
Name is Kristin
```
## Defining an effect
To define your own effect, simply inherit a new type from the base `Effect` type:
```fsharp
/// Logs the given string.
type LogEffect<'next>(str : string, cont : unit -> 'next) =
    inherit Effect<'next>()

    /// Maps a function over this effect.
    override __.Map(f) =
        LogEffect(str, cont >> f) :> _

    /// String to log.
    member __.String = str

    /// Continuation to next effect.
    member __.Cont = cont
```
## Handling an effect
Handling effects  is also easy. The following handles log effects by accumulating strings in a list:
```fsharp
type PureLogHandler<'env, 'ret when 'env :> LogContext and 'env :> Environment<'ret>>(env : 'env) =
    inherit SimpleHandler<'env, 'ret, List<string>>()

    /// Start with an empty log.
    override __.Start = []

    /// Adds a string to the log.
    override __.TryStep<'stx>(log, effect, cont : HandlerCont<_, _, _, 'stx>) =
        Handler.tryStep effect (fun (logEff : LogEffect<_>) ->
            let log' = logEff.String :: log
            let next = logEff.Cont()
            cont log' next)

    /// Puts the log in chronological order.
    override __.Finish(log) = List.rev log
```
<!--stackedit_data:
eyJoaXN0b3J5IjpbMjAyOTI1ODEwMCwtMjAwMTQwMzgxNSwxNj
AzODExNTA1LC02MTM2ODMzMDQsMTY3OTI5ODU5MCwzNTYzMzg0
MzksLTE2MjEzOTcxMzhdfQ==
-->