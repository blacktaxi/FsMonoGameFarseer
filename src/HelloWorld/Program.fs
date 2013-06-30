// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.

open System

[<EntryPoint; STAThread>]
let main argv = 
    use game = new Game.Game1()
    game.Run()
    0 // return an integer exit code
