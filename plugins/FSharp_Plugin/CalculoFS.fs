namespace PluginFS
open System.Collections.Generic
open Latino.Core.Types
open Latino.Core.Runtime

module Matematicas =
    [<LatinoExtension("doble_fs", Help = "Duplica un número usando F#")>]
    let Doble (args: List<LatinoObject>) : LatinoObject =
        let n = (args.[0] :?> LatinoNumber).Value
        LatinoNumber(n * 2.0) :> LatinoObject
