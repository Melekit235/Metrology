let rec nestedFunc1 a b =
    if a = 0 then
        printfn "a is zero"
    else
        match a with
        | 1 -> 
            printfn "a is one"
            printfn "b is %d" b
        | 2 -> 
            printfn "a is two"
            printfn "b is %d" b
        | _ ->
            let mutable c = a
            while c < b do
                printfn "c is %d" c
                for i in 1..c do
                    printfn "i is %d" i
                    let mutable j = 0
                    while j < i do
                        printfn "j is %d" j
                        nestedFunc2 j
                        j <- j + 1
                    nestedFunc1 (c-1) (b+1)
                c <- c + 1

let nestedFunc2 x =
    match x with
    | 0 -> printfn "x is zero"
    | 1 -> printfn "x is one"
    | 2 -> printfn "x is two"
    | _ -> printfn "x is greater than two"

let a = 5
let b = 10

nestedFunc1 a b