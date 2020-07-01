﻿namespace AlgEff.Handler

open AlgEff.Effect

/// Pure log handler.
type PureLogHandler<'ctx, 'ret when 'ctx :> LogContext and 'ctx :> ContextSatisfier<'ret>>(context : 'ctx) =
    inherit SimpleHandler<'ctx, 'ret, List<string>>()

    override __.Start = []

    override this.TryStep<'stx>(log, effect, cont) =

        let step log (logEff : LogEffect<_>) cont =
            let log' = logEff.String :: log
            let next = logEff.Cont()
            cont log' next

        this.Adapt<_, 'stx> step log effect cont

    override __.Finish(log) = List.rev log
