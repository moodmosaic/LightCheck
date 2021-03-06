﻿/// <summary>
/// This module deals with simplifying counter-examples. A property fails when
/// LightCheck finds a first counter-example. However, randomly-generated data
/// typically contains a lot of noise. Therefore it is a good idea to simplify
/// counter-examples before reporting them. This process is called shrinking.
///
/// Read more about how it works here:
/// http://www.dcc.fc.up.pt/~pbv/aulas/tapf/slides/quickcheck.html#shrinking
/// </summary>
module LightCheck.Shrink

open FSharp.Core.LanguagePrimitives

/// <summary>
/// A shrinker for values of type 'a.
/// </summary>
type Shrink<'a> =
    | Shrink of ('a -> 'a seq)

/// <summary>
/// Shrinks to false.
/// </summary>
/// <param name="x">The boolean value to shrink.</param>
let bool =
    function
    | true -> Seq.singleton false
    | _    -> Seq.empty

/// <summary>
/// Shrinks towards smaller numeric values.
/// </summary>
/// <param name="n">The numeric value to shrink.</param>
let inline number n =
    let genericTwo = GenericOne + GenericOne
    n
    |> Seq.unfold (fun s -> Some(n - s, s / genericTwo))
    |> Seq.tail
    |> Seq.append [ GenericZero ]
    |> Seq.takeWhile (fun el -> abs n > abs el)
    |> Seq.append (if n < GenericZero then Seq.singleton -n
                   else Seq.empty)
    |> Seq.distinct

/// <summary>
/// Shrinks a sequence of elements of type 'a. First it yields an empty
/// sequence, and then it iterates the input sequence, and shrinks each
/// one of the items given the shrinker which is passed as a parameter.
/// </summary>
/// <param name="shr">
/// The shrinker function, to be applied on each element of the list.
/// </param>
/// <param name="xs">The input sequence to shrink.</param>
let list xs (Shrink shr) =
    let rec shrinkImp xs =
        match xs with
        | []       -> Seq.empty
        | (h :: t) ->
            seq {
                yield []
                for h' in        shr h  -> h' :: t
                for t' in (shrinkImp t) -> h  :: t'
            }
    shrinkImp xs
