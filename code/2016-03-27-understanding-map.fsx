(**
\---  
layout: post  
title: "Understanding map"
\---
*)

(*** hide ***)
module Main

(**
One important function in functional programming is the `map` function. When I learned F# I must
admit that I had some problems first, understanding it. The problem was, I already knew the `map`
function from dozens of other languages. Or to say it correctly, I mostly learned a wrong explanation
of `map`.

The typical explanation I'm talking about often goes something like this: `map` takes a function and
a `list`. It applies the function to every element in the list, and returns a new `list`.
You will often see examples like this:

    // F#
    let xs = List.map (fun x -> x * 2) [1;2;3;4;5]

    // C#
    var xs = Enumerable.Range(1,5).Select(x => x * 2) // Select is `map`

    // JavaScript    
    var xs = [1,2,3,4,5].map(function(x) { return x * 2 })

All examples start with some kind of array collection that contains the numbers from 1 to 5.
And all of them take a function multiplying the number by two. All of the examples will result in a
new collection containing `[2;4;6;8;10]`.

While this explanation of `map` is *right* for `List.map`, this is *not a right* explanation of `map` in general.
The problem starts when you encounter a functional language, because besides a `List.map` you will also encounter
things like `String.map` or `Option.map`. On top you will also often find the advice that you should provide
a `map` function for every type you create (if possible). When you have a `Result` type you should
also provide a `Result.map`. Also a `Async.map` is a good idea. So if you only knew `map` from the idea of
going through a collection you will probably suffer to understand what `map` is about. If you try to implement
`map` for yourself, you will probably even wonder what `map` anyway should do for an arbitrary type? What is
for example the purpose of `Async.map`? 

To explain what `map` really is about, let's forget about what you already know and start from scratch again.

## Some functions

Before we look at `map`, let's create some simple functions. These functions will be used throughout the article.
*)

// Squares a number: int -> int
let square x = x * x

// Add 10 to every number: int -> int
let add10 x = x + 10

// Returns the length of a string: string -> int
let length (str:string) = str.Length

(**
## List.map

We now assume that we don't have most of the functions from the `List` module. Especially not `List.map`. Sooner
or later you will encounter one problem. With our `square` function we can square an `int`. But our `square`
doesn't work at all with a `list<int>`.

So what do you do if you want to apply `square` to every `int` **inside** a `list`? You sure start looping
over the list, and because we are immutable, we build a new list. As for easiness I write very imperative
code with a loop, without recursion or `fold` or `foldBack`.
*)

let squareList xs =
    let mutable results = []
    for x in xs do
        let res = square x
        results <- res :: results
    List.rev results

(**
So what we now have is a `squareList` function, this function now takes a `list<int>` as input and returns
a new `list<int>`. Our `squareList` function basically does the same thing as `square`, but instead of
`int -> int` we have **upgraded** it somehow to work with `list<int> -> list<int>` instead.

A final note is the `List.rev` at the end, if it is unclear why we need it. `x :: xs` creates a new list,
but it `prepends` elements. We actually cannot add elements to the end. So when we loop over a list like
`[1;2;3;4;5]` we will first `square` 1 and add it to an empty list resulting in `[1]`. Then we `square`
2 and the result is added to `[1]` yielding in `[4;1]` and so on. That's why we have to reverse the list
at the end when we are done!

Some time later we are faced with the problem that we also want to use our `add10` function on a `list`
so we also write a new function for this.
*)

let add10List xs =
    let mutable results = []
    for x in xs do
        let res = add10 x
        results <- res :: results
    List.rev results

(**
Besides that the code is anyway not really nice or functional to begin with, the big problem is that we basically
have written two completely identical functions! The only difference between those two functions is line 4. 
The only thing that is different is the function we call to compute `res`.

Because we like DRY (Don't Repeat Yourself) we do what functional programmers always tell
*Parametrize all the things*. So instead of directly calling our function, we just expect that the
concrete function to execute for every element is just passed as an argument. Or simply we *Abstract* those
two functions. *Abstracting* always means that we put the things that are the same into one function, everything that
is different will be expected as an argument. So what we finally end up with, is our own `map` function.
*)

let mapList f xs =
    let mutable results = []
    for x in xs do
        let mapping = f x
        results <- mapping :: results
    List.rev results

(**
We now can use `mapList` like this.
*)

let listOfsquared = mapList square [1;2;3] // [1;4;9]
let listOfAdd10   = mapList add10  [1;2;3] // [11;12;13]

(**
Currently it doesn't seems like a big difference to other introductions, but let's reconsider what lead to
the idea of creating a `map` function. The idea was. We have a function `int -> int`. A `list<int>` contains
`int` so we could use `square` or `add10` on every element. But in order to apply our function to every
element we have to handle the `list`, and we need to loop through them. Because this process is the same
for every function, we abstract the looping away in it's own function named `map`.

Before we go even deeper in why this is different from other explanation. Let's first look at the signature
of our `mapList` function, and let's just remember the signature.

    ('a -> 'b) -> list<'a> -> list<'b>

## Option.map

Suddenly later when we are programming, we face a new problem. We encounter a `option<int>` value. `option<int>`
contains an `int`. So because it contains an int, we also could use our `square` function on the **inner** value.
But sure, now we have to handle `option`. So what do we do? We can sure unwrap it, and in case of a `Some` we apply our
function to it. But what do we do in a case of `None`? Returning some kind of *default* int doesn't seem to
make sense or like a good idea. So what we will instead do, we just create a new function that will return
an `option` again. In the case of `None` we just return `None` and do nothing.
*)

let squareOption opt =
    match opt with
    | None       -> None
    | Some value -> Some (square value)

(**
Like `squareList` previously we now created a `squareOption`. It is already interesting to see some common
between all those function. `square` could simply square an `int`. `squareList` could square a `list<int>`
and now `squareOption` can square an `option<int>`. Let's go further and let's implement `add10Option`.
*)

let add10Option opt =
    match opt with
    | None       -> None
    | Some value -> Some (add10 value)

(**
Once again we can see the code duplication. So instead of checking again and again for every `option`
if it is `None` or `Some` and only in the `Some` case call a function we starting to abstract it!
Instead of calling our function directly, we once again expect it to be passed as an argument. We will
call this abstract function `mapOption`.
*)

let mapOption f opt =
    match opt with
    | None       -> None
    | Some value -> Some (f value)

(**
We now can use `mapOption` like this.
*)

let OptionSquare1 = mapOption square (Some 5) // Some 25
let OptionSquare2 = mapOption square None     // None
let OptionAdd10_1 = mapOption add10  (Some 5) // Some 15
let OptionAdd10_2 = mapOption add10  None     // None

(**
Let's once again look at the type signature of our `mapOption`

    ('a -> 'b) -> option<int> -> option<int>

Does this already look familiar?

## The commonalities between `List.map` and `Option.map`

Now let's reconsider with what we started. We started with functions like `square` and `add10`.
But those function only could work with `int`. But while we were programming we faced values like
`list<int>` or `option<int>`. To use our functions on those values, we somehow have to **unwrap**
the values. Apply our function to the inner type `int`, and wrap it up again. **unwraping** is
different for every type. For `list` it means we loop through a list. For an `option` it means
we have to check whether it is `Some` or `None`. But we still can think of it as some kind of an
**unwrap** function. Because what we do is, at some point in our function we turn an `list<int>` or
an `option<int>` just to in `int`, so we can use the `int` with our `square` or `add10` function.
But after applying our function we still return the type of what we started. When we started with
a `list<int>` we still have to return a `list` again. When we started with an `option` we still
return an `option` again.

But this is a repetitive task, as this kind of **unwraping** and **re-wraping** is always the same.
It doesn't matter which type we have inside `list<>` or `option`. And it also doesn't matter
which function we use.

That's why we abstract those idea of **unwrapping**, **applying a function**, **re-wrap** into it's
own function and name it `map`. To understand this process further let's look again at the type
signatures of our `mapList` and `mapOption` function.

    ('a -> 'b) -> list<'a>    -> list<'b>
    ('a -> 'b) -> option<int> -> option<int>

This *type-signature* is the essence of a `map` function. Every `map` function has to look like this.
The only part that changes is the *wrapping-type*. So at this point you could probably already assume
how a `map` function for `Result`, `Async` or `Whatever` should look like

    ('a -> 'b) -> Result<'a>   -> Result<'b>
    ('a -> 'b) -> Async<'a>    -> Async<'b>
    ('a -> 'b) -> Whatever<'a> -> Whatever<'b>

## Currying and Partial Application

At this point it is important to talk about Currying. Currying is the idea that there only
exists functions with **one-argument** and they **always** have to return a value. F# is such
a language and does currying automatically.

That is also the very reason why you see multiple `->` inside a function signature. `->` is
basically the symbol for a function. On it's left-side is the input of the function, on the right
side is the output. When we look at a signature like

    string -> int -> float

We often say it has two arguments, a `string` and a `int` and it returns a `float`. But this isn't
quite correct. What we really have is a function that only has one argument a `string` and it will
return a `int -> float`, or in other words. A new function! That is also the reason why functional
languages don't use braces as arguments, it just uses a space. Something like

    let ys = List.map f xs

really means. Execute `List.map f` this returns a new function, and we immediately pass `xs` to
that new function. That's also the reason why we can add braces around the function and the *first*
argument without changing the meaning.

    let ys = (List.map f) xs

Not only that, we also can extract it, and save the *intermediate function* as a new value.

    let newF = List.map f
    let ys   = newF xs

The idea to not pass all needed values is what we call *Partial Application*. The interesting
stuff about all of this is, that with this idea, we can come up with different interpretation
of the same function. And this kind of interpretation is what we can apply to `map`. Actually we
can view `map` as a single argument function, or as a two argument functions. Both have some
different meaning. When we interpret `map` as a single argument function, we now have something like
this

| Function | Input    | Output |
|:--------:|:--------:|:------:|
| List.map   | 'a -> 'b | list<'a> -> list<'b> |
| Option.map | 'a -> 'b | option<'a> -> option<'b> |
| Result.map | 'a -> 'b | Result<'a> -> Result<'b> |
| Async.map  | 'a -> 'b | Async<'a> -> Async<'b> |
| Whatever.map | 'a -> 'b | Whatever<'a> -> Whatever<'b> |

It basically means we can think of `map` of some kind of function that can **upgrade** a function.
If we pass a `int -> string` function for example to `List.map` we get a `list<int> -> list<string>`
function back! If we pass the same function to `Async.map` we get a `Async<int> -> Async<string>`
function back.

So `map` is a way to upgrade both sides (input and output) and add a layer to both sides. There are
two reason on why this concept is important.

1. Code-Reuse. If a type supports `map`, you just can upgrade a function to work with this type.
2. In your own functions, you don't need to care about the layer itself.

## Code Reuse

So let's look again at our starting functions and just use them with the already built-in `List.map`
and `Option.map` functions.
*)

let squareL = List.map square
let add10L  = List.map add10
let lengthL = List.map length

let squareO = Option.map square
let add10O  = Option.map add10
let lengthO = Option.map length

(**
So we just can reuse our three functions. We never have to write special functions that loops
through a list. Or that handles `option`, `Async`, `Result`, we just can **upgrade** any
function we already have written.

## You don't need to care for the layers

This is probably the biggest advantage, as you don't have to care for the *layers*. You want
to convert a list of `int` to a list of `string`. Just write a function that does `int -> string`
no List handling, no looping, no recursion. Use `List.map` and you are done.

And the big advantage. You also can use that function with `Option.map` to turn it into a function
that works on a `option` type. If you pass it to `Async.map` you get a function that can work
on an asynchronous value. You don't need to write code for looping through a list, do pattern
match an option, or write code to handle asynchronicity.

All of this is done for you by the `map` function!

## Async.map

Currently F# don't have a built-in `Async.map` function. So let's create the `map` function
for `Async` ourselves.
*)

module Async =
    let map f op = async {
        let! x    = op
        let value = f x
        return value
    }

(**
So how do we now that we have to implement it in this way? Because that is what the *type-signature*
is telling us. We have to write a function with the signature

    ('a -> 'b) -> Async<'a> -> Async<'b>

1. That means a function with two arguments. The first arguments is a *function* `'a -> 'b`, the second 
   is an `Async<'a>`, and we have to return an `Async<'b>`.
1. Because we have to return an `Async` we start with `async { ... }`.
1. Now `op` is an `Async<'a>`, with `let! x = op` we run the the async operation.
   This will **unwrap** our `Async<'a>` and just returns an `'a`.
1. We can pass that `'a` to our function `f` that converts `'a` to an `'b`.
1. Once we have a `'b` we `return` it. `return` basically wraps the `'b` and adds the `Async<>` layer.

## Stacking Layers

The interesting idea is now. We are not restricted to adding a single layer. We can add as much layer
we want and stack them. For example we could have `option` values that are wrapped inside a `list` returned
by an `Async` operation.

To be more concrete. Let's assume we have some kind of async operation that downloads from a website
(The Async layer). This tries to Parse a table on a website that contains numbers (The List Layer).
But because parsing could fail, for example a table entry is not a number, we wrap it in a `Option`
(The Optional Layer). 

Let's write a *mock* function that returns this kind of data.
*)

let downloadPage = async {
    // Simulating Download, wait 1 second
    do! Async.Sleep 1000 
    // A list of optionals list<option<int>>
    let numbers = [Some 1; Some 2; None; Some 3; None; None; Some 10]
    // return it: This adds async<> layer
    return numbers
}

(**
What we now have is a `Async<list<option<int>>>`. Puh looks complicated! So what do we now
if we want to square the `int` inside our `Async<List<Option<...>>>` construct? We just add
one layer after another to `square`. At first, we do a `Option.map` on `square`. The result
of this is a function that we pass to `List.map` that adds the `List` layer. And once again
the `Async.map` finally adds the `Async` layer.
*)

let squaring = Async.map (List.map (Option.map square))

(**
We now have `squaring` that has the following signature

    Async<List<Option<int>>> -> Async<List<Option<int>>>

We can now do
*)

let data = Async.RunSynchronously (squaring downloadPage)

(**
And `data` will be `[Some 1; Some 4; None; Some 9; None; None; Some 100]`

All `Option`, `List` and `Async` handling was handled for us. We just **upgraded**
`square` with the different `map` functions until it matches our needed signature.

Let's assume we wouldn't have `Async.map`, `List.map`, `Option.map`. We would have needed
to write it like this.
*)

let squaring' input = async {
    let! data = input
    let squared = [
        for x in data ->
            match x with
            | None       -> None
            | Some value -> Some (square value)
    ]
    return squared
}

(**
## Functors

Whenever we have a type with a `map` function we call it a *Functor* if the implementation
of `map` satisfies two laws. Those two laws ensures that `map` is predictable and don't do
additional stuff we didn't expect.

### 1. Law: Mapping `id`

The first rule says that mapping over the `id` function must not change the input. The `id`
function is just a function that returns its input as-is

    let id x = x

So when we write
*)

let xs = List.map id [1;2;3;4;5]

(**
Then `xs` still must be `[1;2;3;4;5]`.

### 2. Law: Function composition

The second rule says that composing two functions and then mapping, should be the same
as mapping over both functions separately.
*)

// 1 solution: compose two functions, and then map
let comp = square >> add10
let cxs  = List.map comp [1;2;3;4;5]

// 2 solution: mapping it two times
let sxs =
    [1;2;3;4;5]
    |> List.map square
    |> List.map add10

cxs = sxs // must be the same

(**
It shouldn't matter if we go through the list take one element and then do `square` and `add10`
in one-step. Or if we go trough our list two times and do it in two separately steps. Both
`cxs` and `sxs` have to return the same result `[11;14;19;26;35]`

Because `List.map` satisfies both laws, we say that `List` is a *functor*.

## Summary

I hope it is now clear why `map` is such an important function. Implementing a `map` function
just means you can **upgrade** already available functions. It opens up a lot of
code reuse as you don't have to write special glue code that handles your type/layer.

It also can make writing new functions easier, as you don't have to care about a layer.
If you find yourself writing a function that has a list as its input and a list as its output
then you are *probably* doing something wrong! The same goes for every other type.

Not only is it easier to just write a function that don't contain any list/looping/recursion
logic. Such a function is even more reusable.
*)
