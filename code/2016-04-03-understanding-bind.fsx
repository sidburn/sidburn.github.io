(**
\---  
layout: post  
title: "Understanding bind"
\---
*)

(*** hide ***)
module Main
open System

(**
In [Understanding map]({% post_url 2016-03-27-understanding-map %}) we learned that implementing 
a `map` function is what we call a *Functor*. In [Applicative Functors]({% post_url 2016-03-31-applicative-functors %})
we extended that idea with the `return` and `apply` function. The next important function in our toolset is
the `bind` function.

## Monads

The combination of `return` and `bind` is what we call a Monad. But currently
I will not consider this as an introduction to Monads at all. If you heard the *Monad* term and
search for an introduction to understand what a *Monad* is you will not find an answer her. If
you already have some basic understanding about the term than this and my two previous blogs
can help to understand the concept. Otherwise if you just try to understand what a *Monad* is
I recommend the following link to understand the problem:
[The what are Monads Fallacy](http://two-wrongs.com/the-what-are-monads-fallacy)

## The Problem

I think it is always good to start with a problem. If we understand a problem first, we usually
have it easier to understand why we are doing something. Currently we have `map` to upgrade functions with
one argument, with `return` and `apply` we could upgrade functions with multiple arguments. So
what is `bind` supposed to do?

Up to this point we only upgraded functions that had normal unboxed input and output types. We always
faced functions like `'a -> 'b`, but never functions like `'a -> option<'b>`, `'a -> Async<'b>`
or `'a -> list<'b>`. But in practice, the latter are quite common. 

A simple example is a function that tries to parse a `string` to a `float`. Because parsing of
a string to a float could fail we usually expect a return type like `option<float>`. Usually we
create a `Double` extension for this.
*)

// Note: .NET System.Double is "float"   in F# and "double" in C#.
//       .NET System.Single is "float32" in F# and "float"  in C#.
type Double with
    static member tryParse str =
        match Double.TryParse str with
        | false,_ -> None
        | true,x  -> Some x

(**
We now have a function `Double.tryParse` with the signature.

    string -> option<float>

I will call such functions *Monadic functions* from now on. All *Monadic functions* expect 
normal input arguments, but return a boxed type, like `option<'a>`, `list<'a>`, `Async<'a>`
and so on.

The problem with such functions is that we cannot easily upgrade them like other functions. For example,
let's assume we have a `option<string>`, and now we want to pass this value to `Double.tryParse`.
As `tryParse` only expects `string` we could `Option.map` `tryParse` so it could work with
a `option<string>`.

But `map` not only adds a `option` *layer* to the input, it also adds it to the output. When we
use `Option.map` on our `Double.tryParse` function, we get a function that looks like this:

    option<string> -> option<option<float>>

The problem is that our *output* is wrapped two times in the same layer. Now we have a
*option containing an option containing a float*. But what we really want is just an `option<float>`.
This is where `bind` comes into the play. The purpose of `bind` is to only *upgrade* the input of a
function because the output of a function already returns an upgraded type. A `bind` function thus
always have the *type-signature*

    ('a -> option<'b>) -> option<'a> -> option<'b>
    ('a -> list<'b>)   -> list<'a>   -> list<'b>
    ('a -> Async<'b>)  -> Async<'a>  -> Async<'b>

## `return` once again

The `bind` function don't stands on it's own. We also need a `return` function. But we
already covered this function in [Applicative Functors]({% post_url 2016-03-31-applicative-functors %}).

## Implementing `bind`

We can implement `bind` in two different ways. It is good to know both as depending on which type we 
have, sometimes the one or the other can be easier.

1. The obvious way. You directly write a `bind` function that is similar to `map`, but instead
   of wrapping the output, you just return the output of the function as-is.
1. You first write a `join`, `concat` or `flatten` function (The exact name of such a function
   usually depends on the type you have). The idea of such a function is to resolve two 
   boxed types just into a single box. After this you just `map` and then `join` the result
   to create `bind`.

The `option` type has the advantage that both implementations are easy, so let's look at how we could
implement `bind` for `Option` in both ways.

### The direct way

The *direct* way can sometimes be nearly identical to `map`. Let's look at the `map` and `bind`
implementation side-by-side.
*)

let mapOption f opt =
    match opt with
    | None   -> None
    | Some x -> Some (f x)

let bindOption f opt =
    match opt with
    | None   -> None
    | Some x -> f x

(**
As you can see, both functions are nearly identical. The only difference is that we just do `f x` instead
of `Some (f x)`. We don't need to wrap the output in a `Some` because our function `f` already returns
an `option`. So we just return it's output directly.

### The `join` way

The other way is to first implement a new function that can turn a `option<option<'a>>` just into a
`option<'a>`. That's also quite easy. We first check our outer-most `option`. If it is `None`
we just return `None`. In the `Some` case we have another option that we directly return.
*)

let joinOption opt =
    match opt with
    | None          -> None
    | Some innerOpt -> innerOpt

(**
Now we create `bind` by just using `map` and `join` the result.
*)

let bindOption2 f opt = joinOption (mapOption f opt)

(**
### Simple usage

Let's test both functions and compare it with a `map` call.
*)

let input1 = Some "abcde" |> bindOption  Double.tryParse  // None
let input2 = Some "100"   |> bindOption2 Double.tryParse  // Some 100.0
let input3 = Some "200"   |> mapOption   Double.tryParse  // Some (Some 200.0)

(**
As we can see from the signature. `input1` and `input2` are just `option<float>`
instead of `option<option<float>>` that a `map` will return us.

The `Option` module already contains `Option.map` and `Option.bind`, so we don't have to
rewrite those ourselves. As another exercise, let's look at a `bind` implementation for `list`.

## `bind` for `list`

Creating a `bind` for a `list` is a case where the first-approach is usually really hard. Let's look
at a `map` implementation for `list` first.
*)

let mapList f inputList =
    let folder x xs =
        (f x) :: xs
    List.foldBack folder inputList []

(**
`Option.bind` was really easy as we could directly return what the call to `f` returned. But for a list
this is not possible. Because in a list we call `f` multiple times for the input list, and the output
of those are collected into a new list.

Because `f` is a *Monadic function* in `bind` it means every call to `f` will return a list. If
we add a list to another list, we get a list of list as expected `list<list<'a>>`. If we try to
return a single `list` instead, it means we have to loop over the result of `f` and add its element
to another list.

Solving that problem inside of `bind` is hard, because `list` is an *immutable* data-structure. With a
*mutable* list (`ResizeArray`) this operation would be quite easy, as we just could call `f x` that
returns a `list` and loop through it and add it to some other list, but with an *immutable* list we
cannot just add elements to an existing element.

When we really want to solve it in one-go we could use a mutable list like `ResizeArray`, otherwise
we have to use two nested `fold` or `foldBack` calls. Instead of nesting it and turning it in a complex
function it is usually better to just extract those operation into it's own function. So we create
a `concat` operation first, that can turn a `list<list<'a>>` just into a single list.

I'm not showing how to implementing `concat` for `list`, as the focus is `bind` not how immutable list
processing works. So for `list` we usually would prefer just a `map` and `concat` implementation for
`bind`.
*)

let bindList f xs = List.map f xs |> List.concat

(**
As you can see. `f` in our example now can be a `'a -> 'b list` function. So it now produces a whole new list
for every input of our starting list, but we still get a single list, not a list of list back.

F# also provides an implementation for this function. But it is named `List.collect` instead of `List.bind`.

## An operator for `bind`

In [Applicative Functors]({% post_url 2016-03-31-applicative-functors %}) we used `<!>` for the `map` function.
And `<*>` for the `apply` function. We use `>>=` as an operator for the `bind` function. But on top of it.
If we write it as an operator we swap the arguments. We expects our type `option`, `list`, `async` on the
left-side and the function on the right-side.
*)

let (>>=) m f = Option.bind f m

(**
## Continuation-passing Style

The reason for this change is that we think of `bind` as some kind of
[Continuation-passing Style](https://en.wikipedia.org/wiki/Continuation-passing_style) programming.
To understand the change, we have to go back at the signature. Up until now i often described `map`
and `apply` by the idea to just pass in the first argument. So when we have

    ('a -> option<'b>) -> option<'a> -> option<'b>

we see it as a function that just *upgrades* the input of a function. But we still have a two argument
function here, and the two argument form is how `bind` is used most often. If we threat it as
a two-argument function we have something like this:

We have a `option<'a>` as an input. And we provide a function `'a -> option<'b>`. As we can see, the input
of `f` is just `'a`. So what we get as the input is the **unwrapped** `'a` that is inside `option<'a>`.

It can help here if we think with piping `|>`. The idea of piping is that we can write the next argument
of a function on the left side. So instead of `f x` we also can write `x |> f`. When we use `bind` with
piping we have something like `x |> Option.bind f`. We also can rearange the *type-signature* to reflect
this style of writing

    option<'a> -> ('a -> option<'b>) -> option<'b>

When we use piping with bind, we get something similar to the above. And probably the order becomes
clearer. We start with a boxed value like `option<'a>`, then our `bind` function somehow extract the
`'a` from our `option<'a>`, this `'a` is then passed to the function `('a -> option<'b>)`. This function
returns an `option<'b>` what is also what `bind` will then return!

But it is important to understand that there is no guarantee that our function will be called at all!
Look again at the implementation of `bind` to understand this. `bind` checks whether we have `None` or `Some`.
In the `None` case it will just return `None` only in the `Some` case it will call `f x` and execute
our function that we passed to `bind`!

Not only that, the *unwrapping* of the `option` is already handled for us by the `bind` function. So we
can pass a function `f` to `bind` that only will be executed if we have `Some value`.

Let's create an example to understand this idea in more depth. At first we create a function that
prints some text to screen and expect the user to enter a float. We try to parse the input as
`float` with our `Double.tryParse` function that returns an `option<float>`.
*)

let getUserInput msg =
    printfn "%s: " msg
    Console.ReadLine() |> Double.tryParse

(**
Now we sure could write
*)

let someInput = getUserInput "Enter a number"

(**
and `someInput` would contain an `option<float>`. We now could use that `option<float>` with other
functions. We just could `map` or `apply` all other functions that are not compatible with `option`.

But instead of doing that, let's pass the resulting `option<float>` directly to `bind`. We then
provide a continuation function to `bind` that only will be executed if we have `Some value`.
The advantage is that our `f` function only sees a `float`, not a `option<float>`. We now
can do something with that `float`. 

Let's write an example where the user inputs the radius of a circle, and we calculate the
area of that circle.
*)

let retn x = Some x
let circleArea r = (r ** 2.0) * Math.PI

let area =
    getUserInput "Enter radius" |> Option.bind (fun userInput ->
        let area = circleArea userInput
        retn area
    )

(**
Let's go through the example step-by-step.

1. At first we just create a function `circleArea` that calculates the area from a given radius.
   For such a function we just expect `float` as input. We usually don't expect `option<float>`
   or `list<float>` as the input.
1. Then we call `getUserInput "Enter radius"`. The user will see "Enter radius: " and he must enter
   something. The input will be parsed as a `float`. We will either get `Some x` back if
   the user input was a `float` or `None` if the input was not valid.
1. This option is then directly passed to `Option.bind` as the second argument. We use the Pipe `|>`
   here to bring the `option` to the left-side.
1. The right-side is now a continuation function. If the `option` passed to `bind` contains `Some x`,
   that means a valid `float`, our continuation function is called and `bind` returns the result
   of our continuation function. If the input to `bind` was `None`, `bind` will immediately return `None`
   without executing the continuation function.
1. Look at the type of `userInput`. It is a `float` not an `option<float>`. We have a continuation
   function that only will be execute if we have a valid `float`. And we can directly work
   with a `float`.
1. In our Continuation function we use the `float` to calculate the area of a circle. As
   we only have `float` not an `option<float>` we don't have to `map` `circleArea`.
1. As you now can see `let area` inside our continuation function is now a normal `float`. But
   now we want to return `area` as the result of our calculation. But `bind` must return
   an `option` value. So how do we do that? We use our `retn` (return) function to convert
   a normal `float` into an `option<float>`
1. Our outer `area` is now a `option<float>` that either is `Some` and contains the calculated area
   for a circle. Or it is `None`, because the user input could not be parsed.

Currently we don't print the result. So let's print `area`. As `area` (outside of the
continuation function) is now a `option<float>` we have to Pattern Match it to see if our computation
was successful or not.
*)

match area with
| None      -> printfn "User Input was not a valid number"
| Some area -> printfn "The area of a circle is %f" area

(**
If the user input was `10` for example, we will see `The area of a circle is 314.159265`, but if we provide
an invalid input, we just see `User Input was not a valid number`. In our example we first had a  
`option` value and passed it to `Option.bind` with `|>`. This happens often, that is why we created
`>>=` previously.

Let's extend that example. We now ask the user for three inputs. And we will calculate the volume
of a cube.
*)

let cubeVolume =
    getUserInput "Length X" >>= (fun x ->
    getUserInput "Length Y" >>= (fun y ->
    getUserInput "Length Z" >>= (fun z ->
        let volume = x * y * z
        retn volume
    )))

match cubeVolume with
| None        -> printfn "Not all inputs were valid"
| Some volume -> printfn "Volume of cube is: %f" volume

(**
As we can see now. We ask the user three times to input a number X, Y and Z. If all inputs were valid. We
just calculate the volume with `let volume = x * y * z`. The important aspect is that all of our values
are always `float` never `option<float>`, because the `bind` operation `>>=` already did the
unwrapping for us.

And probably it now becomes clear why we named our *constructor* `return` (retn). Inside of our
continuation functions we never have lifted values. But at the end of our continuation functions we
always must return a lifted value. So *lifting* and *returning* is always the last statement we do.

Let's inspect the syntax a little bit deeper. Look at the syntax of a normal `let`
definition in F#. Usually a `let` definition contains a name, a equal "=" and a expression that
will be executed. Actually just look at the following two lines and just compare them.

    let x = getUserInput "Length X"
    getUserInput "Length X" >>= (fun x ->

Do you spot the similarities?

1. Both definition have an expression `getUserInput "Length X"` this expression will be executed.
1. In the first example: We only have `=` for assignment, and we assign the result to `let x`.
1. In the second example: We have `>>= (fun x` as we assign the result of the expression to `x`.

So what is the difference between both? 

The first difference is that the statements are just flipped. With `let` we have something like

    let variable = expression

But with our `bind` operation we just have

    expression >>= (fun variable ->

But the more important difference is the result (our variable). In a normal `let` definition we
will get `option<float>`. But with `bind`, we just get `float`. `bind` decides whether our
continuation function should be called or not.

## Computation Expressions

The idea of this kind of continuation-passing style is actually really powerful. So powerful that F#
provides a language construct to let it look like normal code. At first, we just create a class
that contains a `Bind` and `Return` method that we want to use.
*)

type MaybeBuilder() =
    member o.Bind(m,f) = Option.bind f m
    member o.Return(x) = Some x

let maybe = new MaybeBuilder()

(**
As you can see. The `Bind` and `Return` methods are not special. They are just the functions you already
know! After you created a class you must create an object of this class. That is our `maybe`. Now you
can use the following special syntax.
*)

let cubeVolume2 = maybe {
    let! x = getUserInput "Length X"
    let! y = getUserInput "Length Y"
    let! z = getUserInput "Length Z"
    let  volume = x * y * z
    return volume
}

match cubeVolume2 with
| None     -> printfn "User entered some invalid number"
| Some vol -> printfn "Cube volume is %f" vol

(**
So, what happens exactly here? Whenever you use `let!` `Bind` is just called. That means,
if you have `option<float>` on the right side. But you use `let! x`. Then you just get a `float`.
Every code after `let!` is automatically converted into a continuation function that is passed to
`Bind`. The `return` statement (that is only available inside a computation expression) turns
a normal value into a lifted value. In this example it wraps it inside a `option`.

You now can write code as `option` doesn't exists at all. Whenever you have a
function that returns an `option`, you just must use `let!` instead of `let`. The `let!` call uses
`Bind` under the hood. You never need to upgrade functions with `map` or `apply` as you don't
work with lifted values. You can use all your functions directly.

But it doesn't mean that we just erased `option`. `option` is still present, but the handling
of it is done by the `bind` function. Whenever we have an expression on the right side that for example
returns a `None` then the computation stops at this point. Why? Because our `bind` function only calls
the passed in `f` function (the continuation) in the `Some` case.

And it overall also means that the result of a `maybe { ... }` is always an `option`! Because it
is an `option` you easily can use functions defined with a `maybe { ... }` construct in other
`maybe { ... }` constructs.

On top of it you still get the safety that `option` provides you, that means at some point you must
check the value. But it is up to you if you just use a generic check that you implemented in `bind`, or
write your own handling.

What you see here is a basic implementation of the **Maybe Monad**. And it is the implementation of
the second solution I showed in the [null is Evil]({% post_url 2016-03-20-null-is-evil %}) post.

## Defining `map` and `apply` through `bind`

The combination of `return` and `bind` is really powerful. In 
[Applicative Functors]({% post_url 2016-03-31-applicative-functors %}) we already saw that we can
implement `map` through `return` and `apply`. But with `return` and `bind` we can easily implement
`map` and `apply`.
*)

// map with bind operator
let map f opt = 
    opt >>= (fun x -> // unbox option
        retn (f x)    // execute (f x) and box result
    )

// map defined with Computation Expression
let map f opt = maybe {
    let! x = opt  // unbox option
    return f x    // execute (f x) and box result
}

// Apply with bind operator
let apply fo xo =
    fo >>= (fun f ->  // unbox function
    xo >>= (fun x ->  // unbox value
        retn (f x)    // execute (f x) and box result
    ))

// Apply with Computation expression
let apply fo xo = maybe {
    let! f = fo  // unbox function
    let! x = xo  // unbox value
    return f x   // execute (f x) and box result
}

(**
Because of this we always have an *Applicative Functor* when we have a *Monad*.

## Kleisli Composition

Function composition is the idea to create a new function out of two smaller functions. It
usually works as long we have two function with a matching output and input type.

    ('a -> 'b) >> ('b -> 'c)

Because `'b` is the output of anothers function input, we can directly create a new composed
function that goes from `'a` to `'c` `'a -> 'c`. But this doesn't work for *Monadic functions*
as they don't have matching input/output.

    'a -> option<'b>
    'b -> option<'c>

These functions cannot be composed because `option<'b>` is not the same as `'b`. But with our
bind operator `>>=` we can easily pass boxed values into function that don't expect them. Because
of that we also can create a compose function that directly compose two *Monadic functions*.
We use the operator `>=>` for this kind of composition. This kind of composition is also named
*Kleisli composition*.
*)

let (>>)  f g x = (f x) |> g   // This is how normal composition is defined
let (>=>) f g x = (f x) >>= g  // This is Kleisli composition

(**

Now we can compose two *Monadic* functions directly.

    ('a -> option<'b>) >=> ('b -> option<'c>)

the result is a new *Monadic function*.

    'a -> option<'c>

## Laws

We already saw Laws for *Functors* and *Applicative Functors*. The combination of `return`
and `bind` (a Monad) also must satisfy three laws. In the following description I use
*)

let f   = Double.tryParse // string -> option<float>
let g x = retn (x * 2.0)  // float  -> option<float>
let x   = "10"            // string         -- unboxed value
let m   = retn "10"       // option<string> -- a boxed value

(**
But sure, all laws have to work with any function or value combination. But seeing some actual
values makes it easier to understand the laws.

### 1. Law: Left identity

When we `return` (box) a value and then use `bind` (that unbox the value) and pass it to a function.
It is the same as directly passing the value to a function.
*)

retn x >>= f  =  f x  // (Some 10.0) = (Some 10.0) -> true

(**
### 2. Law: Right identity

Binding a boxed value and returning it, is the same as the boxed value
*)

m >>= retn  =  m

(**
### 3. Law: Associative

Order of composing don't play a role. We can pass a value to `f` and the result to `g` and
it has to be the same as if we compose `f` and `g` first, and pass our value to the composed
function.
*)

let ax = (m >>= f) >>= g   // Calling f with m then pass result to g
let ay =  m >>= (f >=> g)  // Compose f and g first, then pass it m

ax = ay // Must be the same

(**
## Summary

With `map`, `retn`, `apply` and `bind` we have four general functions that simplifies working
with *boxed* types like `option`, `list`, `Async` and so on. Whenever you create a new type you
should consider implementing those functions too. Here is a quick overview of those
functions and when to use them.

### `map`

    ('a -> 'b) -> M<'a> -> M<'b>

When we interpret it as a "one-argument" function we can add our boxed type `M` to the input and
output of a function.

Interpreted as a "two-argument" function we can use a boxed value `M<'a>` directly with a function
that can work with the wrapped type `'a`.

### `apply`

    M<'a -> 'b> -> M<'a> -> M<'b>

With apply we can work with boxed functions. We get those as a result if we try to `map` a function
that has more than one argument. Or we just lift a function with `return`. We can view `apply` as
*Partial Application* for *boxed* function. With every call we can provide the next value to a
function that also is a boxed value. In this way we can turn every argument of a function
to a boxed value. A function like `int -> string -> float -> int` can thus be turned into

    M<int> -> M<string> -> M<float> -> M<int>

### `return` or `retn`

    'a -> M<'a>

It just boxes a `'a`
    

### `bind`

    ('a -> M<'b>) -> M<'a> -> M<'b>

Interpreted as a one-argument function, we can upgrade a function like `map`. The difference is
that we only upgrade the input, because the function we have already return a boxed value.

Interpreted as a two-argument function, we see it as a form of Continuation passing style. We
often use piping with `|>` to get the value to the *left-side* and the continuation function on
the *right-side*.

    m |> M.bind f

On top, we give `|> M.bind` it's own operator `>>=`

    m >>= f

This way we have a boxed value `M<'a>`, but our function `f` only receives an unboxed `'a`. In this way
we can work with unboxed values and also use any function without explicitly box them. Because
we must return *boxed* values we usually use `return` to return/box an unboxed value inside of `f`.

The syntax of this kind of continuation-passing style can be improved with a *Computation Expression*.

### Implementations

* `map` can be implemented through `return` and `apply`
* `map` can be implemented through `return` and `bind`
* `apply` can be implemented through `return` and `bind`
* `bind` can be implemented through `map` and some kind of `concat` operation

*)