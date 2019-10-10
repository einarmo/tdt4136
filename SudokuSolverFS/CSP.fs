module CSP

type CSP<'a, 'b when 'b : equality and 'a : comparison> (constraints: Map<'a, Map<'a, 'b -> 'b -> bool>>)=
    let mutable cnt = 0
    let getAllArcs : seq<'a * 'a> =       
        constraints
            |> Map.toSeq
            |> Seq.map (fun (key, next) ->
                next
                    |> Map.toSeq
                    |> Seq.map (fun (key2, _) -> (key, key2))
            )
            |> Seq.concat
    
    let getAllNeighbouringArcs (name : 'a) : seq<'a * 'a> =
        constraints.[name]
            |> Map.toSeq
            |> Seq.map (fun (key2, _) -> (name, key2))

    let selectUnassigned (assignment : Map<'a, 'b seq>) : 'a =
        assignment
            |> Map.toSeq
            |> Seq.find (fun el -> snd(el) |> Seq.length > 1)
            |> fst
    
    let printAssignment (assignment : Map<'a, 'b seq>) =
        assignment |> Map.iter (fun key value ->
            printf "%s: " (key.ToString())
            value |> Seq.iter (fun elem ->
                printf "%s, " (elem.ToString())
            )
            printf "\n"
        )

    let interference (queue : seq<'a * 'a>) (assignment : Map<'a, 'b seq>) : Map<'a, 'b seq> option =
        let rec testPair (arcs: list<'a * 'a>) (lAssignment : Map<'a, 'b seq>) : Map<'a, 'b seq> option =
            match arcs with
            | (arcFirst, arcLast)::arcs ->
                let illegals = Seq.where (fun value -> not (Seq.exists (fun value2 -> constraints.[arcFirst].[arcLast] value value2) lAssignment.[arcLast])) lAssignment.[arcFirst]
                if not (Seq.isEmpty illegals) then
                    let newLegals = Seq.except illegals lAssignment.[arcFirst] |> Seq.cache
                    if Seq.isEmpty newLegals then
                        None
                    else
                        let nextAssignment = lAssignment.Add (arcFirst, newLegals)
                        let nextArcs = (Seq.append arcs (getAllNeighbouringArcs arcFirst |> Seq.except arcs) |> List.ofSeq)
                        testPair nextArcs nextAssignment
                else
                    testPair arcs lAssignment
            | _ -> Some lAssignment

        testPair (List.ofSeq queue) assignment

    let rec backtrack (assignment : Map<'a, 'b seq>) : Map<'a, 'b seq> option =
        cnt <- cnt + 1
        if (Map.forall (fun key value -> Seq.length(value) = 1) assignment) then
            Some assignment
        else
            let variable = selectUnassigned assignment
            let rec testNext (values: 'b list) : Map<'a, 'b seq> option =
                match values with
                | value::values ->
                    Map.add variable (seq { value }) assignment
                        |> interference (getAllNeighbouringArcs variable)
                        |> function
                            | Some next ->
                                backtrack next
                                    |> function
                                        | Some next2 -> Some next2
                                        | None -> testNext values
                            | None -> testNext values
                | _ -> None

            testNext (List.ofSeq assignment.[variable])


    member this.backtrackSearch (initial: Map<'a, 'b seq>) : Map<'a, 'b> option =
        cnt <- 0
        match (interference getAllArcs initial) with
            | Some next ->
                let res =
                    match backtrack next with
                        | Some final -> Map.map (fun key value -> Seq.exactlyOne value) final |> Some
                        | None -> None
                printf "%i calls\n" cnt
                res
            | None -> 
                printf "Initial state invalid\n"
                None







            
            