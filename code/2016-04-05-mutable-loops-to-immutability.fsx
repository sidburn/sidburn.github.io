(**
\---  
layout: post  
title: "From mutable loops to immutable folds"
\---
*)

(*** hide ***)
module Main

(**
When we ask of *key-features* of functional programming, you will probably hear two things most often.
Immutability and recursion. But why is that so? As Immutability also becomes more important in OO
languages you will probably find a lot of reason for that one, but why are recursive
functions so important? The short answer is, because of Immutability! To understand the connection
between those, let's start with some code that uses loops with mutation.

## ResizeArray

Let's start with `ResizeArray`, and let's implement a `map` for it. The interesting idea,
even if we have a *mutable data-structures* we still can write functions that behaves as
if we had *immutable data*. That means, we return new data, instead of *mutating* data.
*)

module ResizeArray =
    let map f oldArray =
        let newArray = ResizeArray<_>()
        for x in oldArray do
            newArray.Add (f x)
        newArray

(**
Now we can do stuff like this.
*)

let numbers = ResizeArray<_>()
numbers.Add(1)
numbers.Add(2)
numbers.Add(3)

let squaredNumbers = ResizeArray.map (fun x -> x * x) numbers

(**
We now have two `ResizeArray`. `numbers` still contains `1`, `2` and `3` while `squaredNumbers`
contains `1`, `4`, `9`. So while this behaves like *immutability*, it still isn't exactly
*immutable*. We also see that in the `map` implementation. In `map` we first create
an empty *mutable* `newArray`. While we loop over `oldArray` we *mutate* `newArray` by adding
the result of the transformation `f x` to `newArray`. But the only reason why we could do that 
is because `newArray` is *mutable*!

But what do we do if we have a true *immutable list* like the built-in F# list? We cannot start with
an `empty list` just loop over `oldArray` and directly add an element to the empty list. We only can
prepend elements, but this would create a whole new list and not *mutate* the empty list.

Sure we can assign the new list to a new variable inside the loop. But this doesn't make much sense
as with the next iteration we will lose our newly created list. The thing is, looping always need
some kind of *mutable* variable outside of the loop that we can *mutate*.

Actually F# supports *mutable variables* so as a quick fix we could define a *mutable variable*
outside the loop.
*)

let mapWithImmutableList f (list:_ list) =
    let mutable acc = []
    for x in list do
        acc <- (f x) :: acc
    acc

(**
It doesn't mean we created a *mutable list* here. We still only have *immutable lists*. 
But `acc` itself is *mutable* and can be changed to point to a new *immutable list*.

With `(f x) :: acc` we always create a whole new *immutable list* by using the `acc`
from the previous *loop iteration*. So `acc` only keeps track of the *last or newest*
`acc`.

This implementation still has a problem as we will also *reverse* the list. The problem
is that we only can *prepend* elements to a list and not *append* them.

But on top, we still relaying on *mutable* variables. Let's first forget about the *reverse*
problem. Here we see a general problem. Looping needs *mutable* variables! Sure, we
also can do just some *side-effects* and don't *mutate* anything, but whenever you really
want to compute something you are forced to use *mutation* if you choose a loop.

Looping constructs in general just don't work nicely together with *immuability*. That's also
the reason why looping is discouraged in functional programming. But it opens up the
question how we solve the problems that *looping* solves. How can we for example go through an
*immutable list* without *looping*? And how can we compute something with every element
without accessing a *mutable variable*?

The answer is: Through recursion!

## Looping with recursion

To understand how we can replace loops through recursion, let's start with something simple. We
start with a typical `for` loop as you can see with `C#`.

    [lang=csharp]
    for (int i=0; i<10; i++) {
        Console.WriteLine("{0}", i);
    }

If we really appreciate *immutability* we cannot directly create such a looping construct.
Because such a `for` loop relies on mutating `i` after every step. But it doesn't mean we can
calculate `i + 1`. We can increment `i`, but we have to assign the result to a new variable.
Technically we could expand the looping construct into separate statements.
*)

let i  = 0
printfn "%d" i

let i1 = i + 1
printfn "%d" i1

let i2 = i1 + 1
printfn "%d" i2

let i3 = i2 + 1
printfn "%d" i3

// ...

(**
Sure, writing stuff like that is simple stupid. We don't want to increment `i` ten times, always
create a new variable, and a new print statement. But it is still interesting that the idea
is the same with our first `map` implementation we did for `ResizeArray`.

We always create a new *immutable state*, by using the previous state. And that is where functions
come into play. Actually we are not limited to just assign the result of a calculation just to a
variable. We also can pass the result to a function.

    let calc = i + 1
    f calc

or better, we can do it in one step.

    f (i + 1)

This is an important idea. It just means that when we call a function, it is also an assign statement
at the same time! With this idea, we can actually create a looping construct just out of functions.
Instead of mutating `i` at the end of a loop and repeating our loop. We just call our function
recursively and pass it the next state as an argument!
*)

let rec loop i =
    printfn "%d" i
    loop (i+1)

loop 0 // infinity loop

(**
Sure, we now have another problem, as we currently have an infinity loop. We now *always* call `loop i+1`.
So we have to add some condition when we really want to loop.
*)
 
let rec loop i =
    if i < 10 then 
        printfn "%d" i
        loop (i+1)

loop 0 // Will now print numbers from 0 to 9

(**
Let's abstract it further. Instead of hard coding `10` we want to provide the end as an argument.
Additionally, we also want to be able to execute *any code* *inside our loop* iteration, instead
of hard-coding the *print-statement*.
*)

let rec forLoop i stop f =
    if i < stop then
        f i
        forLoop (i+1) stop f

forLoop 0 10 (fun i -> printfn "%d" i)

(**
We now basically recreated a `for` loop through a recursive function! We just can provide
the starting and the stop value and just provide a function for the *loop-body* that describes
what should be done at every step. The current `i` is just passed as an argument to our function
`f`. This way we can do something with the value.

Let's improve that solution a little bit. It feels a little bit awkward that we have
to provide `stop` and `f` again as arguments. We can fix this problem by just creating a special
`loop` function inside `forLoop` instead.
*)

let forLoop start stop f =
    let rec loop i =
        if i < stop then
            f i
            loop (i+1)
    loop start

forLoop 0 10 (fun i -> printfn "%d" i)

(**
Our `forLoop` function is not a recursive function any more, instead it contains a recursive `loop`
function. `loop` only contains those variables as arguments that we really expect to change.

This has several advantages:

1. `loop` itself is easier and nearly resembles the first `loop` we started with.
1.  We don't need to pass variables like `f` or `stop` to the recursive call as we don't expect
    them to change.
1.  This reduces the possibility of bugs as we cannot change variables that we didn't expect to change.
    For example, previously we could write: `forLoop (i+1) (stop+1) f`
1.  In other cases it also means we can *pre-* or *post-process* data before or after the loop
    starts/finish.
1.  Sometimes we just want to provide a fixed starting value. For example in a `sum` function
    we provide `0` as the starting value. We don't want that the user must enter `0` as a
    starting value for the recursion call!
1.  Different names for arguments. For example our `forLoop` now has `start` instead of `i`. But
    it doesn't make sense to use `start` inside of `loop`. Clearer names can help in understanding
    code. But it also helps users of `forEach`. As they now see `start` as an argument, not `i`.

The only problem so far is that we only can call functions that do some sort of side-effect.
But we cannot for example create something new out of the looping. For example if we want to
create the sum of all numbers from 0 to 9 we could write something like this in C#

    [lang=csharp]
    int sum = 0
    for (int i=0; i<10; i++) {
        sum = sum + i;
    }

But we now know how to fix that problem. We just add `sum` to the `loop` function
as this is a value that we expect to change with every *iteration*. On top our function `f` must
change. In the body of a loop we have access to `i` and access to `sum` outside of the loop.
With recursion we only get access to those data that we pass to the `f` function. So we also must
pass `sum` as an argument to our `f` function. As we cannot *mutate* `sum` inside `f` we expect that
`f` returns the new `sum` that we then use for the next *recursive call*.
*)

let forLoop start stop f =
    let rec loop i sum =
        if i < stop then 
            // create the next sum
            let nextSum = f i sum
            // next recursive call with updated "i" and "sum"
            loop (i+1) nextSum
        else
            // Return "sum" as soon we reach stop
            sum
    loop start 0

let sum = forLoop 0 10 (fun i sum -> sum + i) // 45

(**
The only thing I dislike at this point is that the usage of our `forLoop` is still limited. We have
the signature.

    start:int -> stop:int -> f:(int -> int -> int) -> int

It makes sense that `start` and `stop` are `int`. But usually we expect that the *looping-body* (`f`)
can work with any type. Or in other words, that `sum` can be of any type. And that a `forLoop`
also can return any type. But currently we are limited to `int`.

It is an `int` because we call `loop start 0` and directly pass `0` (an `int`) as `sum`.
It makes more sense that the user can provide the *initial-value*. And `f` just returns a new value
of the same type. This way, the *initial-value* can be generic.
*)

let rec forLoop start stop init f =
    let rec loop i acc =
        if i < stop then 
            let nextAcc = f i acc
            loop (i+1) nextAcc
        else
            acc
    loop start init

let sum = forLoop 0 10 0 (fun i sum -> sum + i) // 45

(**
We now have the signature:

    start:int -> stop:int -> init:'a -> f:(int -> 'a -> 'a) -> 'a

We now created a `forLoop` just with recursion and without any *mutable variable*. Our loop supports
the creation of any value. The function `f` just returns the next state (`acc`) that is used for
the next *iteration/recursion* and we don't need any *mutable variable* anymore.

We now can create any type not just `int`. We could for example build another list from it in
an *immutable* way.
*)

let listOfNumbers = forLoop 0 10 [] (fun i list -> i :: list)
// [9;8;7;6;5;4;3;2;1;0]

(** Or build a string *)

let stringofNumbers = forLoop 0 10 "" (fun i str -> str + (string i))
// "0123456789"

(** Or start with a value and just repeatedly call a function on it *)

let x = 100.0
let y = forLoop 0 5 x (fun i x -> x / 2.0)
// 3.125

(**
## Creating an immutable `foreach`

So far we only looped over numbers. But usually we want to loop over whole *data-structures*.
In C# we have `foreach` for this kind of idea. For example we can loop over a list in this way.

    [lang=csharp]
    var sum = 0;
    foreach ( var x in list ) {
        sum = sum + x;
    }

We now want to do the same but with an *immutable F# list* instead, so how do we loop an
*immutable list*? Actually the only thing we can do with a F# list is either prepend some element
or use pattern matching to extract values from the beginning.
*)

let numbers1 = [2;3;4;5]
let numbers2 = 1 :: numbers1 // [1;2;3;4;5]

let (head::tail) = numbers2

head // 1
tail // [2;3;4;5]

(**
Doing the extraction with a `let` is usually discouraged because such an operation could fail.
If we have an empty list for example we couldn't extract the first element of a list
(and the code throws an exception). With Pattern matching we can specify multiple cases that we
can check. But as you already can imagine again. This must be recursive again! Because we
repeatedly want to extract one element from `tail` and we sure don't want to write:
*)

let (head1::tail1) = numbers2 // 1 [2;3;4;5]
let (head2::tail2) = tail1    // 2 [3;4;5]
let (head3::tail3) = tail2    // 3 [4;5]
// and so on ...

(**
And on top, we need an exit condition. We want to end as soon we end up with an empty list for `tail`.
Now let's do the same stuff we did with our `forLoop`. We expect some *initial value* that we can
specify as an *accumulator*. The `f` function now just gets the list element and our *accumulator*.
Once we end up with an empty list, we just return the *accumulator*. We call this function `fold`.
*)

let fold f (acc:'State) list =
    let rec loop remainingList acc =
        match remainingList with
        | []         -> acc  // We return "acc" when we reached the end of the list
        | head::tail ->      // extract first element of remainingList
            let nextAcc = f acc head // Compute the next accumulator
            loop tail nextAcc        // Recurse with remaining list "tail" and "nextAcc"
    loop list acc

(**
I also changed the order of the arguments compared to the `forEach` function. Now `f` comes first
followed by `acc` and `list`. Summing a list of `int`s can now be written as:
*)

let sum = fold (fun acc x -> acc + x) 0 [1 .. 9] // 45

(**
So, what is `fold` exactly? `fold` is just the idea of looping through a *data-structure*
in an *immutable way*. Instead of accessing a *mutable variable* that you access and *mutate* while
you are looping, you provide a function as the *loop-body* and an *initial-accumulator*. 

In looping you then have access to the current element and a *mutable variable* that you can
modify. In `fold` your `f` function you get the current element and the last state as
function arguments. Instead of *mutating* a variable outside of a loop. In `fold` you just
return the new state that will be used for the next recursive call.

Your *accumulator* will be returned as a result once you traversed all elements of
your data-structure.

## Creating `map`

Now that we abstracted the recursive looping in it's own function, we don't need to write a
recursive loop function anymore to traverse a list! We just can use `fold` for this purpose.

A first attempt to build `map` could look like that.
*)

let map f list = 
    fold (fun acc x -> // foreach (var x in list )
        (f x) :: acc   //     nextAcc <- (f x) :: acc
    ) [] list          // Start with "[]" as "acc" and go through "list"

map (fun x -> x * 2) [0..9]
// [18; 16; 14; 12; 10; 8; 6; 4; 2; 0]

(**
But we still have the same problem as in the beginning. The list is reversed because we *prepend*
and not *append* elements. As we cannot *append* we have to provide another solution.

Besides a `fold` we usually also provide a `foldBack`. A `foldBack` just traverses a data-structure
backwards or from *right-to-left* instead of *left-to-right*.

But we also cannot easily go through a list backwards, an easy fix to this problem is when we
implement `foldBack` by first *reversing* the input list and then use `fold` on it. After we
have `foldBack` we can define `map` with `foldBack` instead of `fold`.
*)

// Reverses a List
let rev list = fold (fun acc x -> x :: acc) [] list

// foldBack loops through the list from the end to start
let foldBack f list (acc:'State) =
    fold (fun acc x -> f x acc) acc (rev list)

// map defined in terms of foldBack
let map f list =
    foldBack (fun x acc -> (f x) :: acc) list []

// Now we get the right result
let numbers = map (fun x -> x * 2) [1 .. 9]
// [2; 4; 6; 8; 10; 12; 14; 16; 18]

(**
Probably you wonder why we choose different argument orders for `fold` and `foldBack` including the
arguments for the `f` function. In `fold` we first have `acc` then `x`, in `foldBack` it is reversed.
`x` then `acc`. The reason is, the position of `acc` *resembles* the order how we traverse the list.

In `fold` we go from left to right. We start with the initial `acc`, we extract the first value 
from our list and we somehow *combine* it with the already present `acc`. Let's visualize the
steps that code like this do:
*)

let sum = fold (fun acc x -> acc + x) 0 [1..5]

(**
    acc | list        | acc + x
    -----------------------------
    0   | [1;2;3;4;5] | 0 + 1
    1   | [2;3;4;5]   | 1 + 2
    3   | [3;4;5]     | 3 + 3
    6   | [4;5]       | 6 + 4
    10  | [5]         | 10 + 5
    15  | []          | return -> 15

When we use `foldBack` we get the same result, but we go from right to left.
*)

let sum = foldBack (fun x acc -> x + acc) [1..5] 0

(**
    list        | acc | x + acc
    ---------------------------
    [1;2;3;4;5] | 0   | 5 + 0
      [1;2;3;4] | 5   | 4 + 5
        [1;2;3] | 9   | 3 + 9
          [1;2] | 12  | 2 + 12
            [1] | 14  | 1 + 14
             [] | 15  | return -> 15

That's why we use `acc x` in `fold` and `x acc` in `foldBack`. For summing numbers this
doesn't make any difference, but for other operations like `map` it does!

## `length`, `filter`, `iter`, `append`, `concat` and `collect`

Let's create some more functions. As an exercise you also can try to first implement
those functions yourself.
*)

// Length
let length xs = 
    fold (fun acc x -> acc + 1) 0 xs

length [1..10] // 10

// Filter
let filter predicate xs =
    foldBack (fun x acc -> if predicate x then x :: acc else acc) xs []

filter (fun x -> x % 2 = 0) [1..10] // [2;4;6;8;10]

// Iter
let iter f xs =
    fold (fun acc x -> f x) () xs

iter (fun x -> printfn "%d" x) [1..5] // Prints numbers 1 to 5 on a line

// Append
let append xs ys =
    foldBack (fun x acc -> x :: acc) xs ys

append [1;2;3] [4;5;6] // [1;2;3;4;5;6]

// concat
let concat lol =
    fold (fun acc x -> append acc x) [] lol

concat [[1;2;3]; [4;5;6]; [7;8;9]] // [1;2;3;4;5;6;7;8;9]

// collect
let collect f xs =
    map f xs |> concat

collect (fun x -> [x;x;x]) [1;2;3] // [1;1;1;2;2;2;3;3;3]

(**
From the implementation i think only `append`, `concat` and `collect` are interesting.

In `append` you see that you don't have to start from an empty list. Appending two list basically
means you prepend the first list to the second list by going backwards through the first list. So
the second list is your starting *accumulator*.

With `append` in place, `concat` becomes easy. As you just repeatedly append two lists to a single
one.

And once you have `concat`, it is also pretty easy to implement `collect` through `map` and `concat`.

## Summary

We first started with some loops that modifies *mutable variables*. In order to get rid of *mutable*
variables or to work with *immutable data-structures* we need recursion. Without *recursion*
it is basically impossible to effectively work with *immutable data-structures*. But this doesn't mean
we always have to write recursive functions.

We start by implementing a `fold` function that does the same thing as a `foreach` loop just in
an *immutable* way. With `fold` we abstracted the *recursive looping* in it's own function.
New functions can now be implemented on top of `fold` or the function we created with `fold`.

While *recursion* is indeed an important aspect of functional programming, as we just need it to
work with *immutable data*. It doesn't mean we should use *recursion* all over the place. The use
of *recursion* is often a sign that we lack abstraction.
*)
