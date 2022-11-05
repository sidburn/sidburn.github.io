(**
\---  
layout: post  
title: "Higher-kinded Polymorphism: What is it, why you want it"
\---
*)

(*** hide ***)
module Main

(**
One aspect of a programming language that is often noted as important is the idea of *Polymorphism*.
But there doesn't exists just one type of polymorphism. In functional languages *Parametric
Polymorphism* (aka Generics) is often used. Haskell was the first language that introduced
"Higher-kinded polymorphism". Sadly, F# don't support this kind of polymorphism directly. Actually it
only has partial support for it. So let's look in what it is, and why you want it.

## Polymorphism

Before we go deeper let's recap what polymorphism is about. Polymorphism is the idea
that you can write code that looks the same. But it can do different things depending on the
concrete type.

    let x = 1 + 3
    let y = "foo" + "bar"

As we see here, we have a polymorphic `+`. Depending on it's type, it does different things.
It either can add two `int` or add two `string`. It is important to note that the types itself
still remain the same. `+` is polymorphic because it can be used with different types, but every
type can have it's own implementation.

This idea is important because it can greatly help to make code readable. Let's assume we wouldn't
be able to write a polymorphic `+`, so `+` always can only operate on a concrete predefined type.
If that would be true, we actually would need different `+` operators for every type. For example
OCaml doesn't support this kind of polymorphism, so OCaml has two different types for adding
`int` and `float`

    [lang=ocaml]
    let x = a + b  // int
    let y = a +. b // float

So if you want to add a `string` you need yet again another operator/function. So Polymorphism can
greatly help, because we can create the general concept of *add two things*. And we can use this
operation with different types.

## Higher-kinded polymorphism

Now let's assume we want to write a function that just adds two `int` together. We could just write

    let add x y = x + y

But when we inspect the type-signature of this function, we get `int -> int -> int`. The reason for this
is that the type-inference system of the F# compiler defaults to `int`. But actually the type
can change if we use `add` with a different type. For example if we write

    let add x y = x + y
    let result = add 1.3 2.1

We now have `add` with the signature `float -> float -> float`. But it is still important to note that
`add` now only can work with float. Using it like these

    let add x y = x + y
    let r1 = add 1.3 2.1
    let r2 = add 3 4

will create errors at line 3 saying that `3` and `4` was expected to be of type `float` 
but we provided `int` as a value.

The problem we have is that `add` itself is not polymorphic at all. But let's consider, why do we have
that behaviour anyway? The only thing we do is add two things together, adding two things together is
polymorphic, so doesn't make it sense that `add` also should be polymorphic? Well yes, it makes sense,
but this is not what F# does by default. At default it tries to get concrete types or also generic
types. But it cannot automatically create Polymorphic functions that accepts all types that can be
added (+), for instance.

But as said before, F# supports this kind of stuff partly. Indeed we can fix this problem 
very easily with the `inline` keyword.
*)

let inline add x y = x + y
let r1 = add 1.3 2.1
let r2 = add 3 4

(**
Now all compiler errors are gone. Let's look at the type-signature of `add` again.

    'a -> 'b -> 'c (requires member (+))

What we now have is a function that can take two generic values. But not any kind of generics. Both
generics must support the `+` operation. It is also important to look at the return types. `r1` is
of type `float` while `r2` is of type `int`. If you come from a C# background it could probably be that
you are not impressed, but actually such kind of function is not possible to write in C#. In C# you
always have to provide explicit arguments, and you have to write two version of `Add` if the return
type should remain the same.

    [lang=csharp]
    public static int Add(int x, int y) {
        return x + y;
    }

    public static float Add(float x, float y) {
        return x + y;
    }

Actually you cannot write it with a generic type like this:

    public static T Add<T>(T x, T y) {
        return x + y;
    }

The problem with this code is. You cannot add two generic variables! What you really need is the ability
to say: Allow any type that supports the `+` operation.

Probably you will say: Okay but i don't need to write the `int` version. As `int` can implicitly convert
to `float`, so the `float` version also works with `int`. That might be right, but it is not the same, 
your return type will also be `float` not `int` anymore. We could argue with *floating-point inaccuracy*
on why it is not the same, but there is a better way to show the difference. What do you do if your type
supports `+` but don't support a conversion to `float`?

Let's assume we have the following `Vector3` type.
*)

(*** define:vector3 ***)
type Vector3 = {X:float; Y:float; Z:float} with
    static member create x y z = {X=x; Y=y; Z=z}
    static member (+) (a,b)    = Vector3.create (a.X + b.X) (a.Y + b.Y) (a.Z + b.Z)
    static member Zero         = Vector3.create 0.0 0.0 0.0
    static member DivideByInt(a,b) = 
        Vector3.create 
            (LanguagePrimitives.DivideByInt a.X b) 
            (LanguagePrimitives.DivideByInt a.Y b) 
            (LanguagePrimitives.DivideByInt a.Z b)

(**
    type Vector3 = {X:float; Y:float; Z:float} with
        static member create x y z = {X=x; Y=y; Z=z}
        static member (+) (a,b)    = Vector3.create (a.X + b.X) (a.Y + b.Y) (a.Z + b.Z)

We now have our own `Vector3` and implemented `+` for it. The big advantage is now, that
our `Vector3` also can be used with our polymorphic `add` function written in F#. But in C#
you must create a new `Add` function instead, because we cannot convert a `Vector3` to a `float`.
*)

let vec1 = Vector3.create 1.0 1.0 1.0
let vec2 = Vector3.create 2.0 2.0 2.0
let vec3 = add vec1 vec2

(**
Now `vec3` will also be of type `Vector3` containing `{X = 3.0; Y = 3.0; Z = 3.0;}`. But probably
now you will say. Okay, but our `add` function is some kind of silly. We also could use `+` directly
and we wouldn't have the problem at all. So let's create a more advanced function that does a little
bit more. Let's create an `average` function.

To start, let's create a non-polymorphic `average` function that expects a `float list` as input, and
returns the average.
*)

let averageFloat xs =
    let mutable amount = 0   // The amount of values we have
    let mutable sum    = 0.0 // Zero for `float`
    for x in xs do
        sum    <- sum + x    // Add for `float`
        amount <- amount + 1
    sum / (float amount)     // Divide by int for `float`

let x = averageFloat [1.0 .. 100.0] // 50.5

(**
Sure, we also could solve it more functional with recursion and immutable state, but this is not
the point of the post!

So, the question is, how can we made it more polymorphic? Just adding `inline` will not help in that
case and made it automatically polymorphic. The problem is already the third line. We write
`let mutable sum = 0.0`. Or in other words, we create explicitly a `float` at that point. Another
problem is the last line `sum / (float amount)`. As here we convert `amount` an `int` to a `float`.

To get this function polymorphic, we need three polymorphic behaviours that every type could implement
on their own. We need.

1. A polymorphic *get zero*
1. A polymorphic *+*
1. A polymorphic *divide something by an int*

Luckily all three interfaces are already part of the F# language, and we have helper functions for those
operations. A truly polymorphic `average` would then look like this.
*)

let inline average (xs:'a list) =
    let mutable amount = 0
    let mutable sum    = LanguagePrimitives.GenericZero<'a>
    for x in xs do 
        sum    <- sum + x
        amount <- amount + 1
    LanguagePrimitives.DivideByInt sum amount

(** 
To use our `Vector3` type with `average` we have to add the remaining polymorphic `Zero` and
`DivideByInt` members. 
 *)

(*** include:vector3 ***)

(**
`average` is a truly polymorphic function because it can calculate the average of a list
of any type that supports `Zero`, `+` and `DivideByInt`.
*)

let floatAverage  = average [2.0 .. 99.0]            // 50.5
let vectorAverage = average [vec1; vec2; vec3; vec3] // {X=2.25;Y=2.25;Z=2.25}

(**
The big advantage is that we only need to write a single `average` function that can work with
different types. We don't have to create multiple `average` function each for there own type.
If `average` itself only uses polymorphic functions, then it means `average` itself also could
be polymorphic.

But this also means that whenever we create a new type with the correct implementations, we
actually can get a lot of functions for free. Every type that we create that supports `+`, 
`Zero` and a `DivideByInt` automatically gets an `average` function for free!

Or in other words. Higher-kinded polymorphism is about code reuse. By just implementing the
right *glue-functions* it can be that you get hundreds of already pre-defined functions!

As an example you probably heard that `fold` and `foldBack` are very powerful functions, 
and just with `fold` you can implement a lot of other functions like `List.filter`, 
`List.collect`, `List.map` and so on. This is interesting as it means, you could theoretically
provide a single polymorphic `filter` function, and it would work with all types that
implements a `fold` function. The same is true for all other functions that could be
implemented through `fold`. 

This basically would mean you never have to implement dozens of functions that you see in
the `List` module. You just need to implement `fold` and you would get dozens of functions
for free. But currently this is not how it is implemented in F# or how it works. Instead
we have `List.filter`, `Array.filter`, `Map.filter`, `Set.filter`, `Seq.filter` and so on.

Or in other words, every type just implements its own `filter` function, instead that we have
a single implementation of `filter` that could be used polymorphic across all types. That
also means that if you create your own types you have to implement `filter` and all
of the other functions by yourself. But with higher-kinded polymorphism you just need
to implement `fold` for your type, and you would get hundreds of functions for free.

So the big advantage of "higher-kinded polymorphism" is that you get a ton of code reuse.

## Can we solve it with interfaces?

Probably you will ask: *Can we not solve it with an interfaces*? The answer is no. You can achieve
something similar, but not the same. Actually there already exists a solution for the `fold`
example solved with interfaces. It is the `IEnumerable<T>` interface.

`fold` itself is basically just a way to loop over a data-structure. The `IEnumerable<T>` interface
provides the same logic. Once you implement the `IEnumerable<T>` interface that is just a single
method `GetEnumerator` you also get all of the LINQ Methods for free like `Select`, `Where`,
`Aggregate` and so on. In F# you get the functionality of the `Seq` module. So what is the
difference?

The difference is that your type changes from whatever you had to `IEnumerable<T>` (C#) `Seq<T>` (F#).
If you have a `List<T>` (C#) and you use `Select` on it, then you get an `IEnumerable<T>` back.
Or in other words, you loose your original type. If you want to go back to a `List<T>` you have
to convert your `IEnumerable<T>` back to an `List<T>`, `T[]`, `Dictionary<K,V>` or with whatever you
wanted/started. But with Higher-kinded polymorphism instead you would not only get all
of the additional functions for free, your type also would still remain the same.

This means for example if you use a `Set` you just can `filter` it, and directly afterwards you
can use special `Set` methods only available on `Set` like `Set.intersect`, `Set.isSubset` and others.
If you started with an `Array` you can use `Array.blit`, `Array.fill` and other `Array` specific
functions and so on.

Actually it is even hard to say that you get any kind of code reuse with an interface. Sure you
can provide methods that turn something into your `IFace` interface. If you have functions that
only expects `IFace` objects you now can use all of them.

But that isn't really so special. Sure, after i converted something to a `List` i also can use all
of the `List` functions inside the `List` module, what a surprise!

## Summary

F# doesn't support higher-kinded polymorphism directly. It has the features to create this kind
of code with re-usability. You also don't need to implement the `average` function, as F# already
has `List.average` that is polymorphic in the way I showed here. But overall the language itself
was not build up with this feature in-mind, and it also don't make it easy to create polymorphic
functions in that way.

But it is a really important concept, and I think programing languages should try to focus
more on this kind of polymorphic behaviour. If you are aware of this feature, probably you see the
chance of creating your own polymorphic functions and you gain a lot more code reuse.
*)
