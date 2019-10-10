// Learn more about F# at http://fsharp.org

open System
open System.IO
open System.Diagnostics

// Computational expressions are very pretty
let readSudokuFile (path : string) =
    seq {
        use sr = new StreamReader(path)
        while not sr.EndOfStream do
            yield sr.ReadLine ()
    } |> Seq.map (fun str -> str |> Seq.map (fun chr -> int chr - int '0') |> Seq.toArray) |> Seq.toArray

// Black magic
let buildSudokuCSP : CSP.CSP<int * int, int> =
    seq {
        for row1 in 0..8 do
            for col1 in 0..8 do
                yield ((row1, col1), seq {
                    for row2 in 0..8 do
                        for col2 in 0..8 do
                            if (row1 <> row2 || col1 <> col2) && ((int (row2 / 3) = int (row1 / 3) && int (col1 / 3) = int (col2 / 3)) || row1 = row2 || col1 = col2) then
                                yield ((row2, col2), fun (x : int) (y : int) -> x <> y)
                                
                })
    } |> Map.ofSeq |> Map.map (fun key value -> Map.ofSeq value) |> CSP.CSP

let printSudoku (result: Map<int*int, int> option) =
    match result with
        | None -> printf "No solution found\n"
        | Some res ->
            for row in 0..8 do
                if row = 3 || row = 6 then
                    printf "---*---*---\n"
                for col in 0..8 do
                    if col = 3 || col = 6 then
                        printf "|"
                    printf "%i" res.[(row, col)]
                printf "\n"
            printf "\n"

let solveSudoku (path: string) (csp: CSP.CSP<int * int, int>) =
    let raw = readSudokuFile path
    seq {
        for row in 0..8 do
            for col in 0..8 do
                if raw.[row].[col] = 0 then
                    yield ((row, col), seq { 1..9 })
                else
                    yield ((row, col), seq { raw.[row].[col] })
    } |> Map.ofSeq |> csp.backtrackSearch |> printSudoku
    

[<EntryPoint>]
let main argv =
    let csp = buildSudokuCSP
    let stopwatch = new Stopwatch ()
    stopwatch.Start()
    solveSudoku "easy.txt" csp
    solveSudoku "medium.txt" csp
    solveSudoku "hard.txt" csp
    solveSudoku "veryhard.txt" csp
    stopwatch.Stop()
    printf "Completed in %i ms" stopwatch.ElapsedMilliseconds
    0