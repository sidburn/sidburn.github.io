(**
\---  
layout: post  
title: Introduction to F#
date: 2016-03-10 00:00:00
\---
*)

(*** hide ***)
module Main
type DateTime      = System.DateTime
type StringBuilder = System.Text.StringBuilder

(**
When I remember the first time I looked at functional(-first) languages like F#, ML, Haskell and others.
The typical reaction that I had, and I always see from other people is: This is unreadable, it
must be hard to read, it feels complicated and hard. 

After spending some time in F# I cannot agree to that at all anymore. Often the syntax
itself is easier (for example compared to C#), shorter and in my opinion more readable.
The problem is more over that most functional languages shares a syntax that is completely
different compared to languages like C, C++, C#, Java, JavaScript and other more mainstream
languages. The problem is more that it is just unfamiliar.

In this post I want you to give a quick overview over the most common and important concepts.
With this overview it should be easy to understand the most basic part to read 
and understand functional code.

For better understanding I will provide some C# to F# code examples.

## Variables

### Definition C#

Variables are an important concept, in C# you can define variables in two ways. First with an
explicit type. You can optionally initialize it with a value.

    [lang=csharp]
    int    num  = 33;
    string name = "Hello";
    Person person;

The second way is to use the `var` keyword. It uses *type-inference* to automatically determine the
type of a variable. You are also forced to specify a value in this way.

    [lang=csharp]
    var num    = 33;
    var name   = "Hello";
    var person = new Person("foo");

### Definition in F#

In F# we usually only use the second form of definition. But instead of `var` we write `let`.
*)

(*** hide ***)
type Person(name) =
    member val Name = name with get,set
(*** show ***)

let num    = 33
let name   = "Hello"
let person = Person("foo")

(**
We already can see some important differences.

1. Semicolons are not needed to end/separate commands.
2. We don't have to specify `new` to create an object.

We still can add *type-annotations* if we want.
*)

let num    : int    = 33
let name   : string = "Hello"
let person : Person = Person("foo")

(**
## (Im)mutability 

### (Im)mutability in C#

One important difference is that every variable in C# is *mutable* by default. This
means you can change a variable at any time you want

    [lang=csharp]
    num    = 66;
    name  += " World!";
    person = new Person("bar");

In C# you otherwise only can create immutable class fields with the `readonly` keyword.
You cannot create immutable *local-variables*.

### (Im)mutability in F#

In F# on the other hand, everything is immutable by default. You cannot change a variable by default.
If you want to create a mutable variable you have to mark a variable as `mutable`
*)

let mutable num    = 33
let mutable name   = "Hello"
let mutable person = Person("foo")

(**
You change the content of a variable with `<-` instead of `=`. Equal is only used to specify or comparison.
There is no operator like `+=` in F#. F# doesn't try to make mutability convenient.
*)

num    <- 66
name   <- name + " World!"
person <- Person("bar")

(**
## Functions / Static Methods

### Definition in C#
In C# you define *static methods* as part of a *class*.

    [lang=csharp]
    public static class MyOperations {
        public static Add(int x, int y) {
            return x + y;
        }
    }

### Definition in F#
In F# you put functions inside modules.

    module MyOperations =
        let add x y = x + y

We can see once again some important differences.

1. You also use `let` for the definition of a function
2. Arguments `x` and `y` will just be separated by spaces instead of `(x, y)`
3. *Type-inference* also works for functions.
4. There doesn't exists a `return` keyword. The last *expression* is automatically returned as a value.

You also can add explicit *type-annotations*.

    module MyOperations =
        let add (x:int) (y:int) : int = x + y

### Calling functions in C#

    [lang=csharp]
    var result = MyOperations.Add(5, 10);

### Calling functions in F#

    let result = MyOperations.add 5 10

The only difference is that you don't use braces and commas to separate the arguments. You
just provide the arguments as-is.

## Generics

One important concept that you will see more often in F# (compared to C#) is the usage of generics.
Because *type-inference* also works with functions. F# often automatically generalize a function
with generic arguments, instead of specific-types. And overall generics are more important in
functional languages.

### Generics in C#

    [lang=csharp]
    public static T SomeFunction<T>(T input) {
        // Some code
    }

### Generics in F#

    let someFunction (input:'a) = // Some code

Generics in F# are just annotated like normal types. The only difference is that all of them start
with an apostrophe. Instead of `T`, `TIn`, `TOut` and so on, as often used in C#, in F# they will
be just letters `'a`, `'b`, `'c` ...

As stated previously. You also don't need to annotate generics. If you have written a generic function,
F# will automatically infer a generic type for you. So overall you also could just write.

    let someFunction input = // Some code

## Data-Types

Other than object-oriented languages the key concept of *functional programming* is a separation
between *data* and *behaviour*. In *OO* programming we use *classes*. Classes can contain
public/private fields to save data, and provide methods for working with this data.

In a *functional-language* instead we usually define our data-types as separate immutable data.
We then provide (pure) functions that gets immutable data as input, and generate some new data
as output. Because working with data is so important, a functional language offers more than just
classes to define data-types. Besides classes we can use *tuples*, *records* and 
*discriminated unions*.

### Tuples in C#

Tuples are also present in C#. There already exists as a `Tuple` class. But working with them
is not so convenient as in F#. Anyway let's quickly look at how you use them.

    [lang=csharp]
    var position = Tuple.Create(3, 4, 5); // The type is: Tuple<int,int,int>
    var x = position.Item1; // 3
    var y = position.Item2; // 4
    var z = position.Item3; // 5

Tuples are a good way for intermediate types. If you easily want to pass some values as
*one unit* to a function. But more often they are used as a result. So you can easily return
multiple different values from a function.

### Tuples in F#

Working with Tuples is much easier in F#
*)

let position = 3,4,5  // The type is: int * int * int
let x,y,z    = position

(**
You create Tuples just by separating values with a comma. You can extract a Tuple with
a `let` definition. This way you can easily create a function that return multiple data
at once. Tuples don't must contain the same types.
*)

let someFunction x = x, x*2

(**
This function for example returns a Tuple with two elements. The input itself, and the input 
multiplied by Two.
*)

let result = someFunction 10
let x, y   = result

(**
We also can write it in one line
*)

let x,y = someFunction 10

(**
Other than C#, tuples have its own type signature. Instead of a Generic Class like `Tuple<int,int,int>`
a tuple is built into the language itself. They will be represented as `int * int * int` as a type.
`int * float * string * Person` would be a four element tuple (quadruple) that contains an `int` a `float`
a `string` and a `Person` in that **exact** order.

### Records in F#

Working with tuples is good for intermediate function, for example if you create Pipelines like you
see them with LINQ in C#. They are also good for grouping two or three elements, but as soon you have
have more elements, they are unhandy to work with. An alternative to this is a Record type. If
you know JavaScript you can compare them just to an object. Or a hash in Perl. The only difference
is that they are static typed. So you must define a type beforehand. 

Records are planned as a feature in C# 7.
*)

type Human = {
    Id:        int
    FirstName: string
    LastName:  string
    Born:      DateTime
}

let me = {
    Id        = 1
    FirstName = "David"
    LastName  = "Rab"
    Born      = DateTime(1983, 02, 19)
}

let age = DateTime.Today.Year - me.Born.Year
printfn "%s %s is currently %d years old" me.FirstName me.LastName age

let newMe = {me with LastName = "Raab"}
printfn "%s %s is currently %d years old" me.FirstName    me.LastName    age
printfn "%s %s is currently %d years old" newMe.FirstName newMe.LastName age

(**
This code will produce the output:

    [lang=console]
    David Rab is currently 33 years old
    David Rab is currently 33 years old
    David Raab is currently 33 years old

Defining a Record needs explicit type annotations. Creating
a Record is pretty easy. You just use the `{ ... }` syntax. This is nearly identical to JavaScript.
As *functional-languages* prefer *immutability* a Record type itself is also *immutable* by default.
It also has default equality and comparison implementations.

There exists a special *copy and update* operation. It is `{record with newField = newValue }`. You
also can set multiple fields at once. As seen in the example. This creates a new record and doesn't 
modify the old record.

You can access member of a record with a dot. Records also can be deeply nested, so 
you can create hierarchical data-structures.
*)

type Attributes = {
    Strength:     int
    Dexterity:    int
    Intelligence: int
    Vitality:     int
}

type CaharacterSheet = {
    Name:      string
    Attribute: Attributes
}

let warrior = {
    Name = "Conan"
    Attribute = 
    {
        Strength = 1000
        Dexterity = 200
        Intelligence = 3
        Vitality = 1000
    }
}

printfn "%s was a Warrior with Strength of %d" warrior.Name warrior.Attribute.Strength

(**
### Discriminated Unions in F#

A Discriminated Union (DU) also doesn't currently exists in C#, but they are also planed as a feature for C# 7.
A DU is important as they provide a *OR* type. When you look at classes, tuples or records all of them are
basically *AND* types. All of those types group some data-together, but you always have all of them
at the same time. But what happens if you want to express some kind of *Either A or B*? The closest 
thing you can get are *enums* in C#, but *enums* cannot contain additional values for each case.

DU are important, because if a language supports both kinds, we also say that it has an 
*Algebraic type-system*. Let's assume we have a shopping system, and we want to express that a 
user can pay with different methods.

1. **Cash** -- No additional data needed
2. **PayPal** -- We need the email address
3. **CreditCard** -- We need the credit card number
*)

type Payment =
    | Cash
    | PayPal     of string
    | CreditCard of string

let inform payment = 
    match payment with
    | Cash          -> printfn "User payed cash"
    | PayPal email  -> printfn "User payed with PayPal. Email: %s" email
    | CreditCard no -> printfn "User payed with CC. No: %s" no

inform Cash
inform (PayPal "foo@example.com")
inform (CreditCard "123456789")

(**
The above code will produce an output like

    [lang=console]
    User payed cash
    User payed with PayPal. Email: foo@example.com
    User payed with CC. No: 123456789

Here `inform` is a function with one argument `payment`. Still note that we don't need any kind of
*type-annotation*. We use *pattern matching* on payment. Just the fact that we use `Cash`, `PayPal`
and `CreditCard` the F# Compiler can automatically infer that the argument has to be of type `Payment`.

*Pattern matching* is a kind of *switch* statement but more powerful, because it not only matches on
the different cases, you also can extract the additional values that are carried within each case.

Also note the syntax `inform (PayPal "foo@example.com")` We need the braces here not for invocations.
We need them for grouping. This is probably one source of confusion for people coming from C-style
languages. If we wouldn't use the braces and write something like `inform PayPal "foo@example.com"`
we would try to invoke the `inform` function with two arguments. The first argument would be `PayPal`
and the second argument would be `"foo@exmaple.com"`. That would fail because `inform` is not a two 
argument function. We first need to create a value. That is just done with `PayPal "foo@example.com"`
and we want the result to pass to our function. That is why we need to add braces around our call.

This is comparable to just simple maths. `3 + 4 * 5` would yield in `23`. If we otherwise write 
`(3 + 4) * 5` we would get `35`. Braces are just grouping constructs! This becomes more important 
if we have something like these.

    someFunction (Foo x) (Bar z)

This would be a Function call with two arguments. The first argument is the result of `Foo x`, the 
second argument would be the Result of `Bar z`. Coming from a C-style language people often try to read it as

    [lang=csharp]
    someFunction(Foo x)

as a single function invocation with one argument, and they see a trailing `(Bar z)` and they don't know
what it stands for. Actually converting such a function call to C# would result in something like this

    [lang=csharp]
    someFunction(new Foo(x), new Bar(z));

The big advantage of Discriminated Unions is that each case can contain objects, tuples, records or 
other discriminated unions as values. It even can contain itself as an element. In this way you
can easily build recursive data-structures.
*)

type Markdown =
    | NewLine
    | Literal    of string
    | Bold       of string
    | InlineCode of string
    | Block      of Markdown list

let document = 
    Block [
        Literal "Hello"; Bold "World!"; NewLine
        Literal "InlineCode of"; InlineCode "let sum x y = x + y"; NewLine
        Block [
            Literal "This is the end"
        ]
    ]

let rec produceHtml markdown (sb:StringBuilder) =
    match markdown with
    | NewLine         -> sb.Append("<br/>") |> ignore
    | Literal    str  -> sb.Append(str) |> ignore
    | Bold       str  -> sb.AppendFormat("<strong>{0}</strong>", str) |> ignore
    | InlineCode code -> sb.AppendFormat("<code>{0}</code>", code) |> ignore
    | Block  markdown ->
        sb.Append("<p>") |> ignore
        for x in markdown do
            produceHtml x sb |> ignore
        sb.Append("</p>") |> ignore

let html = StringBuilder()
produceHtml document html

printfn "%s" (html.ToString())

(**
Running the above code will produce us the following output

    [lang=console]
    <p>Hello<strong>World!</strong><br/>InlineCode of<code>let sum x y = x + y</code><br/><p>This is the end</p></p>

So we can easily create hierarchical data-structures, and with Pattern Matching we can easily write recursive
function to traverse them.

### List in F#

The example above already introduced lists. Otherwise a list in F# is different to the C# `List<T>` type. 
In C# you create a mutable `List<T>` object and you can directly `Add` items to. In F# on the other hand you
create lists just with the syntax `[ ... ]` (Like in JavaScript). Otherwise elements get separated by `;`
instead of `,`. This is often a source of confusion, because both styles are allowed but they mean something 
different.

    let data = [1;2;3;4]

This is a List of `int`. And it contains four elements.

    let data = [1,2,3,4]

This is a List of a Tuple `int * int * int * int` and it contains a single element. Remember `,` is
for creating Tuples!

Additional lists in F# are also immutable. They also provide default implementations of equality, comparison
and so on. If you want to add elements to a list you have to create a new list. This can be easily done with
`::`.

    let data    = [1;2;3;4]
    let oneMore = 5 :: data

`oneMore` is now

    [5;1;2;3;4]

note that `data` is unchanged and is still a four element list. The way how lists are build (*immutable* 
and as *linked-list*) means adding and removing from the beginning is an efficient operation *O(1)*.

There are various functions inside the `List` module to transform lists itself. With `[|1;2;3|]`
we also can create *mutable fixed-size array*. There also exists a `Array` Module with nearly the same 
functions as in the `List` module.

## Composition and Piping 

The last concepts we look at in our introduction is the concept of *Composition* and *Piping*. Both
are very important in functional languages, as more complex logic is achieved by composing of functions.
Compose ability is actually pretty easy. Let's assume we have a function that takes an `int` as its input
and a `string` as its output. In C# we would usually define such a *method interface* in that way.

    [lang=csharp]
    string SomeMethod(int x);

This could be for example part of an `interface` definition in C#. In F# we would define such an interface
just as

    int -> string

This definition means. A function that has an `int` as an input, and will return a `string`. Note that
we don't specify a function name. Every function itself is actually an interface of its own. Something
like this also exists in C#. Usually expressed as either `Action` or `Func`. We also could have written.

    [lang=csharp]
    Func<int,string>

In C# `Action` and `Func` types are usually used in Methods if we want to expect a function as an argument.
In C# you need `Action` to describe function with a `void` return value.

    Func<int,string>
    string SomeFunction(int x)

And Action means

    Action<int,string>
    void SomeFunction(int x, string y)

In F# we just have a sepcial type named `unit` to express *Nothing*. So we can write

    int -> string
    int -> unit
    int -> string -> unit

The Last line can be read as. A function with two arguments `int` and `string` and it will 
return `unit` (Nothing).

Now let's assume we have two functions with the following signatures

    string   -> int list
    int list -> int

So we have a function that has a `string` as it's input, and a `int list` (List of int) as its
output. Our second functions takes a `int list` as its input, and will produce just a `int`
as its output. Looking at those signatures we now can compose them. Even if we don't now what
those functions do. We just know that the output of the first function can be directly given
as the input of the second function. 

We can directly create a function with a `string` input returning an `int`.
This kind of idea is what we name *composing*. In F# we have a special operator for this
kind of composition. The `>>` operator.

But let's work step by step to it. Let's assume we have a `parseInts` function
that takes a string, splits a string on ',' and parses every number as an `int`
and returns `int list`. The signature would be `string -> int list`.

We then have another function `sumList` that just takes a `int list` and sums all
numbers together returning an `int`. We could use those two functions like this:
*)

(*** hide ***)
type String = System.String

let parseInt str = System.Int32.Parse(str)

let parseInts (str:String) =
    str.Split([|','|])
    |> Array.map parseInt
    |> List.ofArray

let sumList (xs: int list) = List.sum xs
(*** show ***)

let nums = parseInts "1,2,3,4,5"
let sum  = sumList nums // 15

(** We also could create a new function that combines these two steps into a new function *)

let strToSum stringList =
    let nums = parseInts stringList
    let sum  = sumList nums
    sum

(** 
We now have a function `strToSum` that goes directly from `string -> int`

But these kind of operation is actually pretty generic. As this kind of composing works for
any kind of function with any kind of type. In general we can say. When we have two functions.

    'a -> 'b
    'b -> 'c

we can compose those two into a new function

    'a -> 'c

So let's write a `compose` function that does that kind of stuff for us.
*)

let compose f g x = g (f x)

(**
So let's look at the implementation. We have a function `compose` with three arguments. `f`
is expected to be a function. The same is true for `g`. `x` is just some kind of value. What we
first do is

    (f x)

meaning we will call our `f` function with the `x` value. The Result of that is passed to the `g`
function. The result of the `g` function is then returned as a value. We also could have written it
like this.

    let compose f g x =
        let y = f x
        let z = g y
        z

The F# compiler automatically infers that `f` and `g` are functions.
Just by using it like `f x` or `g y` the compiler knows that `f` and `g` must be
functions with a single argument.

But what kind of types do we have here? The answer is, they are generic. When we look at the type
signature that the compiler created for us, it looks some kind of scary first. We have.

    val compose : f:('a -> 'b) -> g:('b -> 'c) -> x:'a -> 'c

Let's go over it step-by-step

| Argument | Signature | Meaning |
|:--------:|:---------:|:--------|
| f | ('a -> 'b) | A function that goes from 'a to 'b |
| g | ('b -> 'c) | A function that goes from 'b to 'c |
| x | 'a | A value of type 'a |
|   | 'c | It will return 'c  |

Just by looking at the types we can examine what the function does. We have `'a` as a value and two
functions. And we need to return a `'c`. So how do we get a `'c`?

At first the only thing that function can do is pass the `x` value (a `'a`) into the `f` function. That will
return a `'b` value. After we have a `'b` value it only can pass that value into the `g` function.
Finally this returns a `'c` that the `compose` function then returns.

We now could use `compose` like this.
*)

let sum = compose parseInts sumList "1,2,3,4,5" // 15

(**
Here we now call `compose` with three arguments. We provide the `parseInts` function itself as a value.
We then provide the `sumList` function as a the second argument. And our third argument is our `"1,2,3,4,5"`
string.

The last thing we can do now. F# supports *omitting* arguments from a function call. If you *omit* a value, you
get a function back with the remaining arguments. Currently our `compose` function is a three arguments 
function. So what happens if we just provide the first two functions as arguments? We get a function back
that is still waiting for the last third argument.
*)

let strToSum = compose parseInts sumList
let result   = strToSum "1,2,3,4,5" // 15

(** 
This kind of composing is so common that we have a special operator `>>` for this. So all we really need to
do is put >> between two functions, and we get a new function back! So what we are doing is

    (string -> int list) >> (int list -> int)

and we just get a `string -> int` back.
*)

let strToSum = parseInts >> sumList
let result   = strToSum "1,2,3,4,5"

(**
So we can easily create new functions out of smaller functions. This is the essence of *functional programming*.
We have *immutable data-types* that gets transformed from one type to another. And we compose functions
together to create new functions. Note that we also could create such a `compose` function in C#.
But because of a lack of some features in C#, such a function is less practical as it seems. 

    [lang=csharp]
    public static Func<A, C> Compose<A, B, C>(Func<A, B> f, Func<B, C> g) {
        return input => g(f(input));
    }

But it is can help to understand what *composition* means.

The remaining part is now *Piping* that is used more often in F#. Piping can be compared with Linux/Bash
Pipes. For example in Bash you can do stuff like this.

    [lang=console]
    cat file.txt | grep "foo" | sort

It basically means that it prints out the file *file.txt* line by line. The output is passed into `grep "foo"`
that only shows the line that contains a `foo`. And that output is finally sorted by `sort`. F# has the operator
`|>` to provide such a functionality. `|>` just means, pass the value on the left to the function on the right.
So instead of
*)

strToSum "1,2,3,4,5"

(** We also could write *)

"1,2,3,4,5" |> strToSum

(** Having this kind of Piping means we also could have written `strToSum` like this *)

let strToStum x = x |> parseInts |> sumList

(** instead of *)

let strToSum = parseInts >> sumList

(** 
Both styles means the same. In the `|>` we just provide the input argument explcitily. `x |> parseInts |> sumList`
also can be read as. Take argument `x` and pass it to the `parseInts` function. The result of `parseInts` is then
passed into the `sumList` function. This kind of style is often what you see with `List` manipulations.
*)

let numbers xs =
    xs
    |> List.map    (fun x -> x + 3)     // Add +3 to every element
    |> List.map    (fun x -> x * 2)     // Multiply every element by 2
    |> List.filter (fun x -> x % 2 = 0) // Only pick even elements
    |> List.filter (fun x -> x > 10)    // Only pick elements greater than 10
    |> List.take   10                   // Only take 10 Elements

let result = numbers [1..100] // [12; 14; 16; 18; 20; 22; 24; 26; 28; 30]

(**
This style of composing is also what you see with LINQ in C# or Java 8 Stream interface. The above
code could also be implemented in this way with C# LINQ feature.

    [lang=csharp]
    public static IEnumerable<int> Numbers(IEnumerable<int> xs) {
        return xs
            .Select(x => x + 3)
            .Select(x => x * 2)
            .Where(x => x % 2 == 0)
            .Where(x => x > 10)
            .Take(10);
    }

    var result = Numbers(Enumerable.Range(1,100));

## Final Note

I covered quite a lot of topics. But I hope that now *functional languages* looks less scary to you. By understanding
all of the topics you basically already made a big step in understanding F# in general.

*)

