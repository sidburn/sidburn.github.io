(**
\---  
layout: post  
title: Understanding Immutability and Pure Functions (for OOP)
\---
*)

(*** hide ***)
module Main

(**
One important concept in functional programming is immutability. But also in
object-oriented programming immutability and so called *immutable objects* getting
more attention. The problem that I see especially from object-oriented programmers
are really bad explanations. A lot of explanation I had see described it like this:
*Just create a class and make all fields readonly (final or const) and 
you have an immutable object*.

Explanations like these are **horrible**. Such explanations are so simplified that I
would even call them *wrong*. So why are they so horrible? Because they don't really
explain anything at all. A programmer new to this concept will just immediately
think: *Uhm, but I want to change things! I want to add data to an array, I want to 
modify things. I want to do some kind of calculations. I don't want to have static non
changing things. Immutability sounds not practical at all!*

So let's see what immutability really means.

## Immutability in a Nutshell

A much more useful explanation is to say that *immutability* is not about *forbidding change*
at all. Instead *immutability* is more on **how** to *handle change*. Immutability
is not about forbidding some kind of operations. You still can add an element to
an array, the difference is that you just do it differently.

In an mutable world you would directly add your element to an array. In an immutable
world you create a new array with your added element instead. The key concept is to
understand that instead of modifying something you create something *new* with your
change applied.

Once you understand it is more about creating *new* things with your changes applied,
the question that arise is more: *Why should that be better?*

## About OO

Before we go into all kinds of explanations we first have to address OO programming. At first,
talking about immutability and OO at the same time is actually a bad idea. The problem is
that immutability doesn't really fit in the OO world. Because of that we should first focus
on immutability and how it works in a functional language. This will be several magnitudes 
easier. Once we understand it there, we go back to the OO world and look how everything
fits in the OO world.

So why does *immutability* not directly fit in the OO world? Because *immutability* is solely
about data-structures. *Immutability* is the core idea that data cannot be changed. Functions
take *immutable data* and they return *immutable data*.

The problem is that in object-orientation you usually don't create data-structures. You
encapsulate and hide data instead. Data-access is often even viewed as *bad*. Often you got told
to create methods instead of providing access to data. This and other things are the reason
why it is hard to *get* the concept of immutability especially as an OO programmer. We will later
talk about this problem in more depth. For the moment we will put objects aside.

## Immutability is about data

So Immutability really means that data itself cannot be changed. But as stated previously, instead
of modifying data itself we call functions that then can return new immutable data. Let's look at
some immutable data-structures.

### `int` is immutable
*)

let x = 5
let y = 10
let z = x + y // 15

(**
You actually should be familiar with this because it even feels natural that `int` is immutable. You
have a lot of operations like `+`, `-`,`*`, `*`, and some more in the `Math` class. All of those
operations take some number, do some kind of operation with it and return something new instead.

`x` stays the same, instead `+` takes two arguments, and produces a new result. We can actually 
treat `+` just as a function that takes two `int` and produces a whole new `int`. As a result 
we get `z` that is `15`. We wouldn't expect that `x` or `y` also get modified at all.

### `string` is immutable

Using `int` to get the feeling of the concept is easy, but it is sometimes hard how this concept
works with more complex types. Additional `int` is in most languages some kind of special *primitive*
type or a so called *value type*. So we threat them anyway as some kind of *special*.

So let's look at `string`. `string` is usually a *reference type* in most languages like any
other class. But at least in Java or C#, they are still immutable.
*)

let a = "foo"
let b = "bar"
let c = a + b // "foobar"

let foo1 = "foo1"
let foo2 = foo1.Replace('1', '2')
let foo  = foo2.Remove(3,1)

(**
In this examples we even already see a little bit of OO, because we call methods that `string` provides.
But looking at the examples we still see that also `string` behaves much like `int`. If we add
two strings together we don't modify a string. Instead we create a whole new string instead.

We can observe the same with our method calls. Calling `foo1.Replace('1', '2')` doesn't change `foo1`
instead we get a new string back with our change applied.

### `list` is immutable

So let's look into a more advanced immutable data-type a `list` in F#. (This is not
`System.Collections.Generic.List<T>`).

    let data = [1;2;3;4;5]

Usually we want operation for List, for example we want to add elements. In F# we could write something
like this:

    let data2 = 0 :: data

In the same spirit like `+` we have `::` for adding an element to the top of a list. But instead
of modifying the list itself, we get a new list back. It is now important to note that now we have
two lists. `data` now contains `[1;2;3;4;5]` and `data2` contains `[0;1;2;3;4;5]`.

From the examples so far we actually can see a *pattern*. All of our functions take some arguments.
But they always *return* us something new with our wanted modification applied. That alone means
we can often identify mutation by looking at the function signature. Functions without a return
value that just return *unit* or similar *void* in C# *often* mutate data. This alone is
not a proof, but a very high indicator.

So, let's assume we want to do some more real-world stuff with our list. Let's assume we want to multiply
each element in an `int list`. Usually in imperative languages like C# you can see something like this:

    [lang=csharp]
    for (int i=0; i<array.Count-1; i++) {
        array[i] = array[i] * 2
    }

but in an immutable world we would create a whole new list with our change applied. Instead of
direct looping we use functions instead. So for example we have `List.map` that does this kind of
operation for us.

    let data  = [1;2;3;4;5]
    let data2 = List.map (fun x -> x * 2) data

After executing we once again have two lists. `data` that still contains `[1;2;3;4;5]` and 
`data2` that now contains `[2;4;6;8;10]`.

### Records are immutable

Let's create another more advanced example. Let's create a `Person` type that represents a Person
in a database.
*)

(*** hide ***)
type DateTime = System.DateTime
(*** show ***)

type Person = {
    Id:       int
    Name:     string
    Birthday: DateTime
    Likes:    string list
}

(** We now could create a Person like this *)

let me = {
    Id       = 1
    Name     = "David Raab"
    Birthday = DateTime(1983, 02, 19)
    Likes    = ["Pizza"]
}

(** 
This is a record in F#, and like all other data-types it is also *immutable* by default.
So now let's assume we want to change some parts.I like "dark chocolate" and "tea" so
let's add them. Because we cannot change our data, we have to create a new record instead.
*)

let me2 = {
    Id       = me.Id
    Name     = me.Name
    Birthday = me.Birthday
    Likes    = "Tea" :: "Dark Chocolate" :: me.Likes
}

(**
What we now have are two separate variables. `me` still represents

    {
        Id       = 1
        Name     = "David Raab"
        Birthday = 19.02.1983 00:00:00
        Likes    = ["Pizza"]
    }

while `me2` represents

    {
        Id       = 1
        Name     = "David Raab"
        Birthday = 19.02.1983 00:00:00
        Likes    = ["Tea"; "Dark Chocolate"; "Pizza"]
    }

Such a *copy & update* operation for records is quite common so F# provides a built-in
language construct for it. We also could have written.

    let me2 = {me with Likes = "Tea" :: "Dark Chocolate" :: me.Likes}

So *immutability* is about data that cannot be changed. But when we want to change something 
we usually call a function that can create something new for us. Let's actually simplify
our example even more. Let's create a `addLike` function instead of using the `copy & update`
mechanism all over our code.
*)

let addLike likes person = {person with Likes = likes :: person.Likes}

(**
What we now have is a function that takes two arguments. A `string` `likes` that we want to add
and a `Person` record as its second argument. The function will then return a *new* `Person`
record. Now we also could add our Elements by using `addLike` instead.
*)

let me2 = addLike "Dark Chocolate" me
let me3 = addLike "Tea" me2

(**
In this example we call `addLike` with `Dark Chocolate` and `me`. And we get a new `Person` back
with our change applied. Then we use `addLike` on `me2` to create our final `me3`.

It can feel a little awkward to create a lot of intermediate variables, but we can get rid of them 
by chaining functions with `|>`. So we also could have written it like this.
*)

let me2 =
    me
    |> addLike "Dark Chocolate"
    |> addLike "Tea"

(**
Here `me` is *piped-into* `addLike "Dark Chocolate"`. This will result in a new `Person` record
that then is once again *piped-into* `addLike "Tea"`. In object-oriented programming we could
achieve something similar if we have a `Person` class with a method `AddLike` that returns a
new `Person` object, instead of modifying some `private` fields. in C# this would result into
something like this

    [lang=csharp]
    var me2 = 
        me
        .AddLike("Dark Chocolate")
        .AddLike("Tea");

This is similar to [Fluent Interfaces](https://en.wikipedia.org/wiki/Fluent_interface).
But the important point is that `me` as an object don't get modified. `AddLike` would return
a whole new `Person` object with your operation applied. Because it is once again a `Person`
you can chain methods. You also can get a *fluent-interface* by just returning *this* after
each modification. It would look the same. But in the end `me` and `me2` would be references
to the same object, and `me` would be changed. 

## Pure functions

In a *functional-only* language we could probably stop at this point. *Data* and *functions*
are clearly separated, immutability is only about *data* that does not change. The big problem
arises if a language also supports classes. Because a class is about *hiding data* 
and additionally contains *functions*, it introduces a lot of complexity. To understand
the reason of this complexity, we first need to talk about *pure* and *impure* functions on
its own.

### Side-effects

*Pure* functions are only those functions that don't have any kind of side-effects. So
what exactly is a *side-effect*? A simple explanation would be: *A function only can
depend on its input*. Calling a function with the same input, *always* has to produce the 
same output. No matter how often, or at what time you call it. We can view `+` as a pure function.

    let x = 3 + 5

No matter how often or at what time we execute the above statement, `x` should always be `8`.
If at some point it is *possible* that it could return something different, we would have
an *impure* function.

In this example it seems even natural that we don't want any *impure* function. A `3 + 5`
that sometimes could return something different sounds horrible. But the truth is, we
often face *impure* functions and we usually also want them. Examples of *impure* functions are.

1. Getting a random number
1. Getting the current system time
1. Getting the user input
1. Reading data from a file
1. Reading data from a network

To deeper understand why they are impure. Which arguments would a function have that returns
a random number? Usually we would say: *Such a function don't need any input*. And that is a problem.
It means, whenever we call a function with no input. It always have to return the same output.
So it just means, we cannot return *random numbers*, because otherwise that statement wouldn't be 
true. That's is also true for the other functions. We cannot for example implement a 
`readFile "file.txt"` function that returns the content of `"file.txt"`. Because that content 
could change every time. And whenever the content of the file changes. `readFile "file.txt"` would 
return something different.

But currently we only only know half of the truth. Because a *function* still can be impure even
following the above rule. There is even a second rule that a *pure function* have to fulfil:
*We always can replace a pure function call with the value it produces, without that it yields any
change to the program*.

That means. Whenever we see `3 + 5`, we also could replace that calculations with `8`. Or if
we see `readFile "file.txt"` we could replace all calls to `readFile "file.txt"` by the value
that the first function call would produce. This also explains better why a `readFile "file.txt"`
would be impure. If we call `readFile` and some time later once again, we would assume that it
returns the new current state of the file. It also could yield an error if the function in the
mean time was deleted. The point is, we expect that the function can return something
different every time we call it.

But this kind of description also eliminates some additional behaviour.

1. We cannot print something to a console
1. We cannot write to a file/database
1. We cannot send data over network

Let's assume we have `someFunciton 5` that always will return `10` but also prints something
to the console. We couldn't replace all calls to `someFunction 5` just with `10` because otherwise
we lose all log statements in our console.

Thinking over it, we could ask the question. Can we even write any useful program without
side-effects? The answer is no. That is the reason why Erik Meijer often say:
*We all love side-effects*. But that doesn't mean we want side-effects happens all
over our code in every function. If a statement like `3 + 5` could yield `10` that would
probably drive a lot of people crazy, me too. We want side-effects but we want to somehow
control them. We want to minimize side-effects as much as possible.

So how do we do that? By letting pure functions return immutable data!

### (Im)mutability and (im)pure functions

One interesting aspect is that both concepts are completely orthogonal. That means, we can have
any combination of those. We can have pure functions that take mutable or immutable data, and return
mutable or immutable data. And we can have impure functions that take and return mutable or immutable
data. The thing is, mutability or immutability doesn't change whether a function is pure or not. This
is important to understand that both concepts don't relate to each other. Let's for example look 
again at the above impure functions.

* A random number generator returns an immutable int/float
* A function returning the current time can return an immutable `DateTime`
* A function that returns the user input returns an immutable string
* Reading a file or from a socket can also return an immutable string
* A function that prints something to the console takes an immutable string
* Sending/Serialization of data over network can take an immutable data-structure

At this point, I cannot stress further how important it is to understand that immutability
is all about data, not about functions or behaviour. We will see later why this is so important!

### Pure functions with side-effects

The last important point is that we can have pure-functions even if they have some kind of
side-effects. A typical example of this is a function that has internal caching with a mutable
variable.

We could come to the conclusion that this is an impure function as another variable as a
side-effects gets changed. But actually, such a function fulfil all rules we have above. Even
the fact that it mutates some variable. It doesn't really matter, as such a function will still
always return the exact same results to its input. And we always also could replace the function
call with its output. 

This is important because people all to often try to look at implementations, but the implementation
itself shouldn't matter at all. The only thing that should matter is how a function behaves. If
a function behaves like a pure function it is a pure function. The same is also true for
mutability. A lot of people try hard to get rid of mutability, sometimes that can lead to
bad performance or in general can make the code harder to understand. For example it is also
fine to have a function with internal mutable state. As long as that function behaves like a pure 
function and even gets/returns immutable data, it is absolutely fine to have mutable local variables.

I would even state that this is a big advantage of F#! For example a lot
of the functions from the `List` module turn a List into a mutable array, do some work on it, and
turn it back into an immutable list. And overall we don't care that it does that. As long as we
use a function and it behaves like a pure function returning immutable data, we are fine with it.

## Benefits of Immutability

To shorten the example. Let's assume everything is mutable and a *reference-type* and it also
applies to numbers. Saying that, lets look at the following code.

    let x = 5
    someFunction x
    let y = x + 3

What value is `y` now? The answer is, we don't know! `someFunction` could have changed `x` to
some other value without that we are aware of it. So after our function did run, we cannot
know what `x` is, so we don't know what `y` is. But what does that overall mean?

Usually we are told that functions, or also classes, methods should be treated as *block-boxes*.
So we should never have to look at how something is implemented. But the thing is, as long we
have mutable data, that concept cannot work. Because as long we have mutable data it means that
a function could do more as documented. We actually can never be sure that `x` don't get changed 
until we look at how `someFunction` is implemented. Lets look at another problem.

    if value.isValid then
        someFunction x
        if value.isValid then
            ...
        ...

So what is the problem here? We are actually accessing `value.isValid` when it is `true`
we enter the code. Once again we have `someFunction` using `value`. But wait, why do we now
re-check `value.isValid`? It could *probably* be that the programmer in charge was drunk, but
wait, can we be sure that the `value.isValid` is still true? In fact, as long as we have
mutability the answer is *no*.

The problem is we don't see the scope of our variables. It could be that our
`someFunction` also has access to our `value` and modifies it. This sounds like a
horrible programming-style but it is not so uncommon as you think. Did you ever had an
array of objects, and returned an object from this array? If *yes*, you are open to such kind
of errors. Because you have two functions that still can access the same object used at
multiple places. And in fact we don't even need an array. Looking at object-oriented languages
like C# nearly *everything* is actually a reference-type. *Objects* itself never get copied,
only references are copied, and the only thing you pass around are references. But hat
also means that every function could hold a reference to some data and directly change
it whenever it wants to!

So shortly, we cannot know if `value` still contains the same data. It already could have changed
multiple times. This kind of possibility even raises with *multi-threaded* code. And
I'm not talking about *thread-safety* or *race-conditions* here. `value` could be thread-safe
and still changed in the mean time. The thing is, mutability basically makes any kind of code
hard or nearly completely unpredictable.

The problem is, this kind of problem grows the bigger our program becomes. Multi-threading
also increase that kind of problems by several magnitudes. And this is the overall problem.
With more code we anyway face problems of designing and maintaining programs. Mutability
just can create hard to track errors. It can become insanely hard to reason about some
kind of code if at every blink of an eye every value can be changed at any time. Immutability
overall can make code easier to read and maintain.

We also can gain other benefits out of it, like easy do/undo systems, backtracking in recursive
functions for free, and a lot of other stuff.

## Disadvantages of Immutability

Nothing in the world really just have only benefits. Everything in the world has its advantages and
its disadvantages. So what are the disadvantages of immutability?

Mainly it is performance.
Some people think that *copying* is the often problem or *memory*, but that isn't true. For example
let's look at the list example. A lot of people assume that by adding an element to a list a whole
list itself has to be copied. But that isn't true at all. For example adding an element to the top
is an *O(1)* operation. It only can be made so efficient *because* of immutability. An immutable list
is really just a data-structure that contains an element and a reference to another list. 

That's why adding/removing from the top is efficient, instead of adding/removing at the end like
many people knew it from types like `List<T>` in C#. The only reason why you could safely reference
another list is because of immutability. With mutable data this wouldn't be possible as
a list can change. So sharing data with immutable data is very safe. That's also the reason
why you probably hear often that immutability works better with multi-threaded system. Or
functional languages have advantages with multi-threaded systems. It is because immutable data
are preferred and used in such languages.

But it doesn't change that there sometimes exists a problem where this is still a bottleneck
or the culprit to performance problems. The problem with immutable-data is that you have to build
them incrementally. A List with 1 Million elements is really build just as

    let x = 1 :: 2 :: 3 :: 4 :: ... :: []

or in other words. a lot of copy and create options. Sure a compiler or a runtime could have
some optimization. F# probably have them for lists, but that overall doesn't change that 
immutability can sometimes lead to such problems. That is also the very reason why we have 
a `StringBuilder`.

Also a `String` is immutable but concatenating a lot of strings can create a lot of garbage
throw-away objects. A `StringBuilder` can actually close that bridge. A `StringBuilder` uses
a mutable string, and once you are done, you can get an immutable `string` back.

Other problems can arise that some problems or algorithms can be hard to implement with
immutability. I just want to point again at what was said for *pure* functions. If you encounter
such problems you always can convert some kind of data to some kind of mutable data. Do your
operation, and convert it back to a immutable data-type.

So it is still important to understand that not everything is shiny and automatically better.
Immutability can sometime have it's own problems, but there exists solutions for it.

## Immutability and OO

Finally, we now have every knowledge to talk about immutability in object-oriented programming
and why it is so damn hard. First, let's reconsider what an object is.

The fundamental thing
of object-oriented programming is to hide data and instead provide methods that do
some stuff. We even have rules like *Law-of-Demeter* or *Tell don't ask* that express it.
An object is not about asking it form some data, we usually just call a method to
tell it that it should *do* something.

Or in other words. Objects are just collection of functions. And here starts the problem. We
actually learned that immutability has nothing to-do with functions at all! Immutability is 
about data not functions! Functions sure can be *pure* or *impure* but once again, we also 
learned that it doesn't matter at all for immutability. In fact we even consider it as good
if we have side-effects that returns immutable data. That is how to solve the problem of
side-effects. But just having data is usually discouraged in OO. OO has even it's own term
for it. It is named the *Anemic Domain Model* to express if we have classes that just contains
data.

So, if object-oriented programming don't try to use data explicitly, if we only have objects
that provides us functions (methods) to call. How on earth can we even talk about
*immutable objects*? What should that thing even be? Does it even makes sense to talk
about *immutable objects*? If we only provide methods, doesn't it make more sense to talk
about *pure* and *impure objects* instead?

To better see the problem, let's look at at the Random class.
*)

let rng    = System.Random()
let random = rng.Next();

(**
Do we consider `rng` to be immutable or not? Let's look what we have. Besides the usual
method inherited from `object` we only have three additional methods. `Next`, `NextDouble` and
`NextBytes`. `rng` don't have any data or additional properties. We can call `Next` and
we get an immutable `int` back. Besides that we cannot see any difference at all that `rng`
itself changed at all! From the outside it looks like an immutable object!

Sure we have knowledge on how a random class works. Usually we have an internal private field
that holds the last generated number, with this the next number will be created when we call
`Next`. But the point is, we cannot see that. Theoretically the implementation could also 
use no mutable field at all. It could just use the current time to generate a random number
instead. So that `Next` is impure, but don't have any mutable field.

From the outside the only thing we could say is that `Random` has three *impure* functions.
And the object itself looks like immutable. We cannot see that any changes at all happens!

So do we consider `Random` immutable or not? Actually if you really expect an answer, there
isn't really one. Sure we could look at the implementation of it, but that is really bad,
we shouldn't needed to look at some kind of implementation to determine if something is immutable
or not. And as already explained above, it is anyway not a good idea. We should view something
as immutable or pure by looking at how it behaves, not how it is implemented.

So, now we are in a dilemma, how do we solve it? One thing we could do is to broaden the view of
what an immutable object is. So we only consider something as immutable only if it has pure
functions. As soon as we have one impure function on an object, we have to think that there
exists a possibility that a private property could be modified.

Let's look at another example that I saw some time ago. Someone provided a class like this
*)

type MutableSite(url:string) =
    member val Url  = url
    member val Text = ""  with get,set
    member this.Download() =
        use wc      = new System.Net.WebClient()
        let content = wc.DownloadString(this.Url)
        this.Text <- content

(**
So, we have obviously a mutable object, right? We have an mutable `Text`. To fetch the current
site we call `Download` that mutates `Text`. So let's look how that person made it immutable.
*)

type ImmutableSite(url:string) =
    member val Url = url
    member this.Text() =
        use wc      = new System.Net.WebClient()
        let content = wc.DownloadString(this.Url)
        content

(**
So, he just eliminated the `Text` field. Instead he created a `Text` method that would
directly download the site and return the content. Obviously he thought that now he
had an immutable class. And actually that things is just silly. Both version don't
differ at all!

What is the difference between a `Text` field that always can return another string after
we called `Download`, or a `Text` method that directly return a new string whenever 
we call the method? There is no difference at all between both version. The problem
is that `Text` always can return something different. If it
is either a mutable field or an impure method doesn't matter at all! Actually it even could
also just be a property that could do this kind of stuff, so it doesn't even look any different
to a normal mutable field instead of a method call.
*)

type SiteWithProperty(url:string) =
    member this.Url  = url
    member this.Text
        with get() =
            use wc      = new System.Net.WebClient()
            let content = wc.DownloadString(this.Url)
            content

(**
The thing is, he thought he made any improvement just because he eliminated a mutable field,
but actually that change don't matter at all. Whether you
have a `Text` field that can change, a `Text` property that changes, or a `Text` method,
in the end, `Text` always can return something different if you try to access it. So it
improves nothing at all. We don't get any *benefits* at all that we should get
by imposing immutability.

This example just shows how hard it is to reason about immutable objects. The problem is
the combination of functions and data in one container like a class. And there is even another
problem. Actually it is just fine to have *impure functions* that return immutable data. But
how do we do that if we consider *impure functions* on an object as bad?

Actually in functional programming we don't have that problem at all. As every function stands on
its own. Sure we group them in Modules, but it doesn't mean a function is part of some kind
of structure. We can reason about every function separately. We can have pure and impure functions.
And none of those changes the fact that we have immutable data. But in a class you combine
functions with some kind of data in one container, the result is that we have to view an object
as mutable as soon as it provides an impure method. The reason is that it behaves exactly
like a mutable field would do.

So how do we create our impure functions in object-oriented programming? As we learned,
we just need them to do anything useful. Just eliminating all kind of impure functions
doesn't help us to solve any problems. The only way out of it is if you write static methods
for impure functions. In this way you can separate impure functions from pure functions
and an object could be considered as pure/immutable as long it only has pure methods.
So let's consider how a good immutable object should look like.

## How to Design immutable objects

1. An immutable class don't have *hidden* (`private`) fields. `private` in the sense of hidden fields
not exposed to the user. Sure a class can have `private` fields for its data. But a class always
have to provide access to the data through a *readonly getter*. If you have *hidden* fields not
exposed to the user, we cannot be sure that an object is *immutable* at all.
1. A class should only contain *pure functions* (methods). We don't knew if an impure function
modifies probably some hidden field or not. And it also doesn't matter. As soon we have a method
that can return something different on every call we also cannot view it as *immutable*. If
a field got changed alongside it or not doesn't matter at all. We judge *immutability* on how
it behaves, not in how it is implemented. Because functions and data are mixed together in a class.
We have to view every *impure method* as a violation against *immutability*.
1. All *impure* functions should be static methods on a class, or extracted into it's own class.
Let's look at `DateTime` as an example. For example we have `DateTime.Now` or `DateTime.Today`. Those
are impure properties as they always return a different `DateTime` whenever we call it. But once we
have a `DateTime` object we only have *pure methods* operating on it. All data are accessible
through getters. All methods are *pure*.
1. As we learned at the beginning, immutability is not about forbidding change, so an immutable
objects should have a lot of methods that gives us easy ways to create new objects with our needed
modification. If you don't provide them, it will probably painful to work with your objects. You
can look again at `DateTime`. We have rich ways like `Add`, `AddDays`, `AddHours`, `AddMinutes` to 
create new DateTime objects. All of those methods return a new `DateTime` instead of mutating a field.

So let's reconsider the `Site` class above. How should an immutable `Site` class looks like?

    type SiteImmutable(url:string, content:string, size:int) =
        member val Url     = url
        member val Content = content
        member val Size    = size
        static member Download(url:string) =
            use wc      = new System.Net.WebClient()
            let content = wc.DownloadString(url)
            SiteImmutable(url, content, content.Length)

So what we really have is a class with our immutable fields. Our member fields
cannot be changed later as they are immutable. Our class constructor has to be pure, the
same as all methods. The creation of our immutable object is handled by a *static impure method*
`let site = SiteImmutable.Download("http://example.org")`

Let's for example consider we later want an `Update` method, so we can re-fetch the `content` of a
`site`. Instead of providing an *impure* `Update` method we have to provide an *impure static method*
that does this for us.
*)

type SiteImmutable(url:string, content:string, size:int) =
    member val Url     = url
    member val Content = content
    member val Size    = size
    static member Download(url:string) =
        use wc      = new System.Net.WebClient()
        let content = wc.DownloadString(url)
        SiteImmutable(url, content, content.Length)
    static member Update(site:SiteImmutable) =
        use wc      = new System.Net.WebClient()
        let content = wc.DownloadString(site.Url)
        SiteImmutable(site.Url, content, content.Length)

(**
So if a user wants to update the content of an object he can do something like this
*)

let site = SiteImmutable.Download("http://example.org");
// Later...
let updatedSite = SiteImmutable.Update(site)

(**
## Conclusion

Immutability itself is actually an easy concept. The problem starts when we don't separate
data and functions clearly from each other like OO programming does it. To really embrace
immutability in OOP you have to forget a lot of stuff you were taught that should be good. Create
pure data-objects as much as possible. Don't implement *impure* methods on such data-objects.
Instead create *impure static methods*. Those should be as small as possible with as
little logic possible. They should return an immutable data-objects as soon as possible.

A good place for *impure functions* are *static methods* or either create special
*impure/mutable* objects instead. But don't try to implement a lot of logic for them,
provide methods to convert an mutable object to an immutable object. `StringBuilder`
is a good example for an mutable object that fixes the performance problems for creating
complex strings. Once you are done you convert a `StringBuilder` instance to an
immutable `string`.
*)
