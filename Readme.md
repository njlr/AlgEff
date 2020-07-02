# AlgEff - Algebraic Effects for F#
## What are algebraic effects?
Algebraic effects provide a way to define and handle side-effects in  functional programming. This approach has several important benefits:
* Effects are defined in a purely functional way. This eliminates the danger of unexpected side-effects in otherwise pure functional code.
* Implementation of effects (via "handlers") is separate from the effects' definitions. You can handle a given effect type multiple different ways, depending on your needs. (E.g. one handler for unit tests, and another for production.)
Algebraic effect
<!--stackedit_data:
eyJoaXN0b3J5IjpbLTU0NjkyNzA1OSwtMTYyMTM5NzEzOF19
-->