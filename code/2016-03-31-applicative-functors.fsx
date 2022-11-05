(**
\---  
layout: post  
title: "Applicative Functors"
\---
*)

(*** hide ***)
module Main

(**
In my previous blog "[Understanding map]({% post_url 2016-03-27-understanding-map %})" I
introduced the `map` function and described that implementing `map` and fulfilling two laws
we get what we call a *Functor*. As we already can guess from the title. An
*Applicative Functor* is some sort of *extension* to that idea.

## Problem with `map`

It might be that you have noticed one problem with `map`. `map` only can work with
one-argument functions! The definition of `map` expects a function `'a -> 'b` as it's
first argument. So we can **upgrade** one-argument functions but what happens
if we want to upgrade two, three or four argument functions?

## Some dummy functions

Once again we will create some dummy functions with more than one argument to see
how we can work with them.

*)

// int -> int -> int
let mul x y = x * y

// int -> string -> string
let repeat count (str:string) = String.replicate count str

(** Some simple usage of those: *)

mul 3 7        // 21
repeat 3 "abc" // "abcabcabc"

(**
## Currying again

But wait, didn't we previously said that there doesn't really exists functions with more than
one argument? Are not all functions just one argument functions, and that two/three/four... arguments
function are really just functions that return another function? Yes, it is and that is also the
reason why we can pass any function to `map`. But probably you will still be irritated. `map`
has clearly a signature of `'a -> 'b`. So how can we pass a `int -> int -> int` function to it?
Shouldn't we need a `map` function that expects something like `'a -> 'b -> 'c` as it's first
arguments? Before we answer that question, actually just let's partial apply one of our function
with a `map` function and let's see what we get.
*)

let optionMul = Option.map mul

(**
When we expect the signature of our `optionMul` function we now get a new function that looks like this.

    option<int> -> option<(int -> int)>

## What happened?

So what happened exactly? And why could we pass `mul` (`int -> int -> int`) anyway to `map` that expected
a `'a -> 'b`? The big answer is, it's all because of currying. As said once before. A definition like
`int -> int -> int` can be really interpreted as `int -> (int -> int)`. The braces are actually optional
as `->` is right-associative. So what is `mul` really? It is a function taking an `int`, and returning
`int -> int`. The important point is. Functions are also just types!

And that is why this function also can be passed as a function that expect `'a -> 'b`.
The generic `'a` will be replaced with `int`, while `'b` will be replaces with `int -> int`.

And probably it now makes sense on why we get our result. Remember that what we get back from `map` is
just the input and output wraped with our type. So when we call `Option.map` with one argument, we
get a function back with it's input and output wrapped. 
        
    option<'a> -> option<'b>

So when we pass `int -> int -> int` then we pass `int` as the type for `'a` and `int -> int` for `'b`.
That's why we get back

    option<int> -> option<(int -> int)>

## What does `option<(int -> int)>` mean?

The question that really starts to beg is. What the hell does `option<int -> int>` anyway mean? And how
do we work with such a construct anyway?

Actually the answer is easy, and it the same way unhelpful. The answer is `option<int -> int>` is just an
optional that can contain a function, or not. Just remember what `option` is about. A `option<int>`
means we either have an `int` or not. Now we have the same, just for a function!

Even answering on how you can work with it is easy. The same as with any other option! You have to
*Pattern match* it. The only difference is that instead of for example an `int` you get a function
that you can execute in the `Some` case.
*)

let seven = optionMul (Some 7) // returns: option<(int -> int)>
match seven with
| None   -> printfn "Nothing to do"
| Some f -> printfn "Executing f: %d" (f 3)
// prints: Executing f: 21

(**
The bigger problem is that all of this question and answers are currently *unhelpful* because
their are the wrong question. We really have to start at the beginning and rethink:
**Which result do we expect after upgrading a two-argument function?**

## Which result do we expect?

Let's rethink the purpose of `map`. `map` is the idea that we can just **upgrade** an existing
function and add the `option` handling for us. It just turns a

    int -> int

into a

    option<int> -> option<int>

So we get a new function that now can work with `option`. We just **upgraded** the input and output
and added a `option`. So if we have a function like

    int -> int -> int

why not just upgrade every element, and turn it into something like this

    option<int> -> option<int> -> option<int>

The question is, how can we achieve that? We sure could start writing a `map2`, `map3` or `map4`
function. But those functions would probably end up in being much the same. Not only that, it also
can get harder and harder to write a function that handles three, four or more `option` at the
same time. On top of that, it doesn't really feel so much flexible, isn't there some better way
so we can handle functions with arbitrary arguments? Sure there is! 

## Introducing `apply`

The solution to our problem is that we just write a function that can handle the output of
our `map` function. Let's work with the `repeat` function this time, and let's also pass in the
first argument.
*)

let optionRepeat3 = Option.map repeat (Some 3)

(**
As we can see we start with a `int -> string -> string` function. But the interesting thing is, we
ended up with `option<(string -> string)>`. Where is our `int` argument? We already applied
that argument when we called `map`. We only need to pass in the remaining arguments.

In some way you can view `Option.map` as *Partial Application*. But it does not just *Partial Apply*
one value to a function, it additional upgrades the input handling of `option` for us. So the only
thing we need to write is a function that can handle a `option<(string -> string)>` function. But how
do we handle such a function?

There are two ways we can handle this construct.

1. We write a function that expects the lifted function (`option<(string -> string)>`)
   and the next argument `string`, and we execute our functions inside the `option`.
1. Transform the whole `option<(string -> string)>` just into `option<string> -> option<string>`

So which one seems easier or more useful? The funny answer is, both operations are the same! 

Let's go over the first idea. Should we expect just a `string` or an `option<string>`? The whole idea
with `map` so far was that we can apply `option` values to function that don't support them. So
it makes more sense when we expect a `option<string>`. If we would expect normal values we 
wouldn't need to `map` a function in the first place! So what we need to implement is a function
expecting `option<(string -> string)>` as the first argument, and `option<string>` as the
second argument. What do we return? As we just execute the first argument, we will return 
`option<string>`. So overall we get

    option<(string -> string)> -> option<string> -> option<string>

Our second idea was that we somehow transform `option<(string -> string)>` (input) to a new function
`option<string> -> option<string>` (output). If we write the output just to a whole function signature,
we also get.

    option<(string -> string)> -> option<string> -> option<string>

What we see here is "Currying" once again. With Currying it not only can be that we can interpret the
same function differently, we also can come up with different ideas, that in the end is the same
with another idea. This kind of idea can sometimes simplifies the implementation.

For example let's stick with the second idea. We want to transform the input to another function.
But when we write the type-signature of our function that we need to implement, it is just the same
as a two argument function. So we start with

    let apply optionF optionStr =
        ...

So what we now have is a function as the first argument `option<(string -> string)>` that expects
a `string`. And we have a `option<string>` as it's second argument. What we now have to do is
unwrap both optionals, and when we have `Some function` and `Some string` we can execute our
function with our value. Actually, there exists 4 possible combination. So we write

    let apply optionF optionStr =
        match optionF,optionStr with
        | Some f, Some str -> ??
        | Some f, None _   -> ??
        | None _, Some str -> ??
        | None _, None _   -> ??

So we *pattern match* both values at once, in our first case we have a function, and
a string, so we can execute the inner function with our passed in value. We must return
an `option` again, so we end up with

    | Some f, Some str -> Some (f x)

All other cases are actually the same. What do we do if we don't have a function, or we don't
have a value? Or we don't have both? Well, then we can't execute our function, so all
of the other cases will return `None` instead.
*)

let apply optionF optionStr =
    match optionF,optionStr with
    | Some f, Some str -> Some (f str)
    | Some f, None _   -> None
    | None _, Some str -> None
    | None _, None _   -> None

(**
So, now we have written a way we can handle the output of `Option.map repeat (Some 3)`. Should we now
write a way to handle `Option.map mul (Some 3)`? When we actually look at the type-signature of our
`apply` function, it is much more general as we might think. It's type-signature is.

    option<('a -> 'b)> -> option<'a> -> option<'b>

That's also why I directly named it `apply` not `applyRepeat`. If you look over the code it makes
sense. Because `optionStr` is nowhere used that restricts it to being a `string`. We just pass
it as the first argument to the inner function. So our second argument just must be the same
as the input type. It might sense to rename the `optionStr` argument just to `optionX` instead.

But probably you might notice another similarity. Here is the signature of `map` and our
`apply` function side-by-side.

    ('a -> 'b)         -> option<'a> -> option<'b>
    option<('a -> 'b)> -> option<'a> -> option<'b>

In that sense, we can say. `apply` does the same as `map`. The only difference is that
it already expects a **upgraded** function instead. But those two functions now works
nicely together.

Because if we pass a function with more than one argument to `map` we get something back that
we can pass to `apply`. By calling `map` we provided the first `option` value. And `apply`
expects the next `option` value.

Now let's try to use `apply` with our `optionMul` function. We first can call `OptionMul (Some 7)`
that will return us an `option<int -> int>`, the result of this can then be used with `apply`.
*)

let optionMul2 = optionMul (Some 7)        // option<(int -> int)>
let resultMul  = apply optionMul2 (Some 3) // option<int>

(**
We also can write everything in one step, instead of creating the intermediate functions. Not only that,
let's even inline the `map` call.
*)

let resultMul2 = apply (Option.map mul (Some 7)) (Some 3)

(**
This doesn't seems very readable, but we will work on that soon. Let's first understand what exactly happens.

1. `Option.map mul (Some 7)` is first executed. It will **upgrade** mul and we provide `Some 7` as
   the first argument to the `mul` function. This will return a `option<(int -> int)>` function.
2. The `option<(int -> int)>` is passed as the first argument to `apply`, the second argument to apply
   is `Some 3`. This will return just an `option<int>`

Currently we **upgrade** `mul` and execute `mul` in one step, because we provide all arguments. But
how can we just **upgrade** `mul` without executing it? Before we do that, let's look in how we can
make the execution more readable.

## Defining your own Operators

In F# we can define our own operators. Operators are basically just two argument function. But instead of 
writing `f x y` an operator is written between (infix) two arguments `x f y`. The value
left of the operator is the first argument, the value right of an operator is the second argument.
So instead of calling `Option.map f x` let's create an operator for `Option.map`.
*)

let (<!>) f x = Option.map f x

(**
We now can use our first improvement. Instead of `Option.map mul (Some 7)` we now can write

    mul <!> (Some 7)

Our whole line turns now into

    apply (mul <!> (Some 7)) (Some 3)

But once again. Writing `apply` in front looks ugly, so let's also create an operator for our
`apply` function.
*)

let (<*>) fo xo = apply fo xo

(**
We now getting the following line:
*)

mul <!> Some 7 <*> Some 3 // Some 21

(**
The nice thing is now. With `<!>` we just can `map` a function (left-side) and on the right side
we provide an optional value. In this example `mul <!> Some 7` will return `option<(int -> int)>`
and this is the input to `<*>`, because it stands on it's left-side. And we provide `Some 3`
as the next value.

This is nice because it resembles the normal way how we call a function. Normally we would do
*)

mul 7 3

(**
But what happens if we have `optional` values? We just write
*)

mul <!> Some 7 <*> Some 3

(**
Sure, normally you wouldn't wrap the values directly, you just would have variables that contain
optionals. So if you have `x` and `y` that are just `int` you can do

    mul x y

But if your `x` and `y` contains `option<int>` instead, you just write

    mul <!> x <*> y

Probably you will ask, how can we handle functions with three or four arguments. Easy!

    someFunction <!> x <*> y <*> z <*> w

Why does that work? Because once again of currying! If you start with let's say a `int -> int -> int -> int`
function, Then after the first `map` you get back `option<int -> int -> int>`. But as we already learned.

`int -> int -> int` is compatible with `'a -> 'b`. That's the reason why you can pass a `option<int -> int -> int>`
also to a function expecting `option<'a -> 'b>`. What now happens is that your `apply` will now return
a `option<(int -> int)>`. Or in other words. With `apply` you *Partial Apply* one value after another
to a **wrapped** function. Whenever you use `apply` or `<*>` you just provide the next value of the wrapped
function inside `option`.

But currently, with `map` and `apply` we map a function and directly pass values to it. What do we do
if we just want to **upgrade** a function without executing it? With what we have so far, we usually
write some helper functions `lift2`, `lift3`, `lift4` and so on.
*)

let lift2 f x y     = f <!> x <*> y
let lift3 f x y z   = f <!> x <*> y <*> z
let lift4 f x y z w = f <!> x <*> y <*> z <*> w

(**
What we now get are the following functions.

    ('a -> 'b -> 'c)             -> option<'a> -> option<'b> -> option<'c> 
    ('a -> 'b -> 'c -> 'd)       -> option<'a> -> option<'b> -> option<'c> -> option<'d>
    ('a -> 'b -> 'c -> 'd -> 'e) -> option<'a> -> option<'b> -> option<'c> -> option<'d> -> option<'e>

Those functions basically do what we first thought of `map2`, `map3`, `map4` and so on. But such functions
are easily implemented with `apply` in-place. Once again it helps by looking at those function
with Currying in mind. All of those functions makes more sense if we just *Partial Apply* the first
argument. What you see then is that we turn a two, three or four argument function just
into a new function where every argument should be a `option`.

Implementing `apply` is more helpful as we can directly `map` and `apply` any function with arbitrary
arguments, without *Partial Applying* functions. And we still can create easily `lift2`, `lift3` or
`lift4` functions to upgrade functions as a whole. 

Some note if it is not obvious. Usually we don't implement a `lift1` because that is what `map` does!

## The `return` function

Currently we always created all **lifted** values directly. For example if we needed an `option<int>`
we directly wrote `Some 7` to create it. Let's rethink this process. Let's assume we just
have an int like `7`, now we want to upgrade the value. `Some 7` creates an `option<int>` but what
do we do if we want a `list<int>`, `Result<int>` or a `Async<int>`? Sure upgrading an `int` to
`list<int>` is still easy `[7]`. But instead of doing it manually, why not create some kind
of *constructor* that does that for us?

Based on the context such a function is usually called `pure` or `return`. Even if `return` seems
a little bit strange we will pick this one. Later in some other blogs it will become more obvious why
we name it `return`. 

But because `pure` and `return` are both reserved words in F#, we have to slightly
change the name. So we just use `retn`. The *type-signature* of a `retn` function always looks like
this.

    'a -> option<'a>
    'a -> list<'a>
    'a -> Result<'a>
    'a -> Async<'a>

It is pretty-easy to implement `retn` for our option type.
*)

let retn x = Some x

(**
Looking at the previous examples we now also could write
*)

mul <!> retn 7 <*> retn 3 // Some 21

(**
This is probably not such a big surprise. But once again we should consider that `'a` also
could stand for a function. We not only can upgrade values to a type. But also functions.
*)

retn mul

(**
So why do we want to do that? Well we could use `map` or `<!>`. But consider that with `map`
we **only** can upgrade functions. `retn` is more basic as it can **upgrade** every value.
While it seems we don't need `retn` for functions, sometimes we are just interested in just
upgrading a function as raw as possible. The difference becomes more obvious when we compare
both operations.
*)

retn mul       // option<int -> int -> int>
Option.map mul // option<int> -> option<(int -> int)>

(**
So with `retn` we just do the bare minimum to upgrade a value/function. One
interesting aspect is, that we don't need `map` at all. In fact. We can create `map`
out of `retn` and `apply`! As we can see `retn mul` returns `option<(int -> int -> int)>`
and we already saw how we can work with such values. We just can use `apply` to *Partial Apply*
the first inner value. So overal we also could just write.
*)

retn mul <*> retn 7 <*> retn 3 // Some 21

(**
It basically means. `map` is the same as `retn` and `apply` once! We actually could have
implemented `map` like this.
*)

let mapOption f x = retn f <*> x

(**
`retn` is just such an easy function that usually it doesn't seems like much value. But
actually that is quite the reason why it's so good. At first how easy it is just depends on the
type you have. But yes, `retn` is often a very easy implementation. On top of it, it
also can happen that `apply` is sometimes easier to implement as `map`. So it is quite
good to know different ways to implement the same function.

## What can we do with all of this?

Currently we only have written the Applicative Functor for the `option` type. But you can think
of this extension for every type that you usually also can write a `map` function for. This
is a general technique not limited to `option`.

So what can we do now, with all of this? This is actually a solution of the `null` problem
that I described in [null is Evil]({% post_url 2016-03-20-null-is-evil %}). The problem with
`null` is that everything can be `null` and you have to add checks everywhere. Replacing
it with `option` has some advantages, as you only have to check for `Some|None` if you
also expected a `option`. So you only need checking where you expect it. But this can be still
to tedious. Often we want to write code and don't bother with `null` or `option` at all.
Our `mul` and `repeat` functions are such examples. We just expect arguments that are 
`int` and `string`. But what happens if for some reasons you have option values and you still
want to use them with `mul` or `repeat`? Without the idea of our *Applicative Functor*
we either have to:

1. Unwrap all optionals and do the checking
1. Write a `mul` function yourself that expects two `option<int>`

Both solutions are very tedious and can become very annoying. Like `null` checking is always
annoying. So you end up with either.

### 1. Unwrap all optionals beforehand
*)

// Assume we have two optionals from somewhere else
let x = Some 3
let y = Some 7

match x with
| None   -> printfn "None"
| Some x ->
    match y with
    | None   -> printfn "None"
    | Some y -> 
        // Finally we can use `mul`
        printfn "Result: %d" (mul x y)

(**
### 2. Rewrite a `optionMul` function
*)

let optionMul x y =
    match x with
    | None   -> None
    | Some x ->
        match y with
        | None   -> None
        | Some y -> Some (x * y)

(**
Both solutions seems dull. The first solution becomes annoying. Even if we only have to do
add checks for functions/types that are `option`. It still is an annoying task mostly
because it is a repetitive task. The second solution is even more worse
as we don't have any code-reuse at all. We just have to write the whole function from scratch
again. 

The bad part is, that such a function contains more `option` handling to what it even does.
So with our *Applicative Functor* we just can write a normal function, that knows nothing about
`option`, and we later just upgrade it.
*)

mul <!> x <*> y
retn mul <*> x <*> y
lift2 mul x y

(**
All three ways are identical they lift `mul` so we can pass `option` values as arguments.
Sure at some point in your program you probably want or must check the `option`. But it
is up to you where you do it. You can do your whole computation first, and only later check
once if you got `Some value` or `None`. Theoretically it means you can write your whole
program, and it only contains a single `option` check at the end.
*)

let parseInt str = 
    match System.Int32.TryParse str with
    | false,_ -> None
    | true,x  -> Some x

let userInput = System.Console.ReadLine() |> parseInt

let multipliedBy3 = mul <!> userInput <*> retn 3
let repeatedAbc   = repeat <!> multipliedBy3 <*> retn "abc"

match repeatedAbc with
| None     -> printfn "Error: User Input was not an int"
| Some str -> printfn "%s" str

(**
So if the user input is "3" we will see `abcabcabcabcabcabcabcabcabc`. If a user don't provide
an input that can be converted to an `int` we will see: `Error: User Input was not an int`.

The whole idea is probably why some people don't see the benefit of `option`. Most
people just see: *Okay instead of checking for `null` I do check for `Some` or `None`.
 Why is that better?* 
 
Well, using `option` already provides some benefits, as you can't
forget the checks, but the real advantage is that they are values on it's own, and you
can write such an *Applicative Functor* around `option` that supports upgrading any function
to the `option` world and do all the checking for you. That's the real benefit of using `option`.

## Applicative Functor Laws

In [Understanding map]({% post_url 2016-03-27-understanding-map %}) we already came across
two laws that `map` should satisfy. As we now introduced two new functions `return` (retn)
and `apply` there also exists some laws they have to satisfy until we can call it a
*Applicative Functor*.

### 1. Rule: Identity

This basically refers to the first law of a functor. We said that mapping over the `id`
function should not change the value. Because `map` can be implemented in terms of
`return` and `apply` the same law must be hold.

    let x = Some 10
    let y = retn id <*> x
    x = y // comparing must be true -- here it will be (Some 10)

### 2. Rule: Order of **upgrading**

It shouldn't matter if we first calculate `f x` and then `retn`. Or if we `retn` `f`
and `x` separately, and then do the calculation

    let x = retn (mul 7 3)
    let y = retn mul <*> retn 7 <*> retn 3
    x = y // Both must be the same -- here it will be (Some 21)

### 3. Rule: Partial Applying

That one probably needs some more explanation. Usually we can *Partial Apply* a function
by just omitting values. For example `repeat 3`. But what is if you want to *Partial Apply*
the second argument? Here are two solutions in how we can write it.

    let repeatAbc = fun x -> repeat x "abc"
    let repeatAbc x = repeat x "abc"

So after that we can just call `repeatAbc 3`. The thing is we expect the same results regardless
if we Partial Apply the first or second argument first. As long the arguments are the same,
the result should be the same. The same rule must hold when we additionally lift `repeat` to an optional.
*)

let ax = retn repeat <*> Some 3
let  x = ax <*> retn "abc"

let ay = retn (fun x -> repeat x "abc")
let  y = ay <*> retn 3

x = y // Both must be the same -- Here it will be -- Some "abcabcabc"

(**
### 4.Rule: Composition

This rule comes from normal function composition. Let's say we have two functions. One adds "1"
to a value, another one adds "2" to a value. Function Composition says that it doesn't matter
if you execute the first function on a value, and then the second function on the returned
value. Or of you first compose both function and give it the value.
*)

let add1 x = x + 1
let add2 x = x + 2

// First executing add1, and pass the result to add2
let nx = add2 (add1 3)       // 6

// First compose add1 and add2 then provide the value
let ny = (add1 >> add2) 3    // 6

nx = ny // Both must be the same

(**
This makes sense as composing is just executing two functions in sequence and passing the return
value from the first function to the next function. But those law must still hold true if we
lift/box our functions into another type like `option`.

One note first. Operators are really just functions with two arguments. And they can be lifted too!
Normally we write an operator infix (between two arguments). But we also can write it like a normal
function if we add braces around the operator. Thus both following lines are the same.

    let h = f >> g
    let h = (>>) f g

The second style of writing can be used to lift an operator. With `retn (>>)` we can upgrade `>>`.
Normally `>>` would take two functions as arguments, and returns the new composed function.

    ('a -> 'b) -> ('b -> 'c) -> ('a -> c')

But when we use `retn` on it, we just can `apply` our arguments that now also can be
`option<'a -> 'b>`. Instead of `retn (>>)` and `apply` twice we also could use `lift2`
so we would get a compose function that looks like this.

    option<('a -> 'b)> -> option<('b -> 'c)> -> option<('a -> c')>

With that in mind, our forth rule says, that we also must ensure that composed lifted
functions still behaves the same, as if we just execute both functions directly in sequence.
*)

let oadd1 = retn (fun x -> x + 1)
let oadd2 = retn (fun x -> x + 2)

// First executing oadd1, and pass the result to oadd2
let ox = oadd2 <*> (oadd1 <*> retn 3) // Some 6

// First compose oadd1 and oadd2 into a new function, then provide the value
let oy = (retn (>>) <*> oadd1 <*> oadd2) <*> retn 3 // Some 6

ox = oy // Both must be the same

(**
## Summary

We started with `map` as a general function to *upgrade/lift/box* normal functions into
some other types. But `map` only handles one argument functions in a way we would expect.
But because we have currying, and there only exists one argument functions anyway, we still
could pass functions with *more than one-argument* to our `map` function. But instead of 
a value, we get a lifted function back instead. To handle lifted functions we came up
with a `apply` function.

As we later saw, we basically don't need `map`. We always can create `map` in terms of using
`retn` and `apply` once. With our *Applicative Functor* in-place we now can *upgrade/lift/box*
function with arbitrary arguments. We also can easily create `lift2`, `lift3`, ... functions.

With user defined operators like `<!>` for `map` and `<*>` for `apply` we also can easily
*upgrade/lift/box* functions inline, without the need to save the intermediate functions.

In this introduction we only saw the usuage with the `option` type. But in general this
idea works also for other types. While the technique how to implement an *Applicative Functor*
is the same. The meaning of it changes between types. Currently with `option` we basically
have written a solution to the [null is Evil]({% post_url 2016-03-20-null-is-evil %}) problem.
*)