(**
\---  
layout: post  
title: "Exceptions are Evil"
\---
*)

(*** hide ***)
module Main

(**
Most people today agree that `null` is evil, and they try to get rid of them. One technique that
most people prefer is to throw an exception in the case of an error, or if we cannot return a valid
value from a function. The problem is, exceptions are not any kind better than `null`, and
they don't solve any problem that `null` introduced.

In my previous post [null is Evil]({% post_url 2016-03-20-null-is-evil %}) i mentioned seven
problems that `null` introduced. So let's look if *exceptions* solve one of those problems.

## 1. You cannot see if a function throws an exception

When you look at the type signature of any function, you just see something like

    PersonId -> Person

The same way that you cannot see that `null` could be returned. The same way you don't know
if an exception could be thrown or not.

## 2. We are not forced to try/catch

You are not forced to add any kind of `try/catch`, the same way you are not forced to add
a `null` check. If you forgot to add your `null` checks, you end up getting `NullReferenceException`.
If you forgot to adding `try/catch` you end up with `XYZException`. Replacing a `NullReferenceException`
just with another kind of `Exception` isn't anyway better.

## 3. Every function could throw an Exception

The big problem of 1. and 2. combined is that you defensively start to check all return values
from a function if it contains `null`. And you also have to check every argument if it contains
`null`. This leads to numerous `null` checking throughout your code. But why do you add those
checks in the first place? Because you want to prevent `NullReferenceException`. So how is a
function that sometimes returns `null` that can lead to a `NullReferenceException` anyhow better
as a function that sometimes throws `XYZException` directly?

Not only does it not solve the problem at all. You still have to add your checkings. But instead of
`null` checks throughout your code. You wrap your code in `try/catch` blocks. Why is checking for
null bad

    let result = SomeFunction()
    if result <> null then
        // Some code if everything was right
    else
        // Error condition on null

and suddenly wrapping your code in a `try/catch` anyhow better?

    try
        let result = SomeFunction()
        // Some code if everything was right
    with
        | exn -> // Error condition on Exception

## 4. We cannot skip the checking

Probably you would assume that exceptions solve it, but actually, they don't really provide an improvement.
Sure, you don't have to wrap a try/catch block around your code. Exactly the same as you don't have
to write an explicit `null` check. So what happens in both cases?

In both cases an exception is thrown either a `NullReferenceException` or probably you throw some other kind of
`Exception`. Yes, you can catch your `Exception` further up the stack. The same way as you can catch
a `NullReferenceException` further up the stack. There is no difference at all here.

But overall, this was not meant with *skipping*. The idea of *skipping* was that you can do the null check
at some later point where it makes sense. That doesn't mean only "further up the stack". The idea is that you
pass the whole error as a value around, as you can do with `Optional`, and additional you are forced
to check the condition of your `Optional` at compile-time.

## 5. and 6. We can pass functions/objects that throws exception around

At default you don't pass `Exception` types as values around. You wrap your code in a `try/catch` and that's
it. You also cannot implicitly pass an `Exception` as a valid value to a function that expects a `Person`. With
`null` you can do that, that's why we have to also add `null` checks for function arguments.

So it seems we are not affected if we throw exceptions. But that is wrong. An object itself contains *methods*.
And every method on an object could throw an exception. 

With `null` you have to check every argument if it is `null`. With *exceptions* you have to additional add
try/catch blocks if you call a method on an object. Because you pass objects around, and objects have *methods*
that could throw exceptions when invoked. You end up with the same problem.

## 7. Happy Path Coding

It seems *Exceptions* solve the problem of Happy-Path coding. But it really does not. Yes, you are not forced to add
a `try/catch` around every function directly. You just can use one `try/catch` around the whole code and catch
**any exception**.

Absolutely, and the thing is. **Any exception** also includes `NullReferenceException`. So if you like to have
*exceptions*. No problem, just return `null` from your functions and don't add any `null` checks.

## Summary

It seems many people forget about why `null` is bad. `null` is bad because **they throw exceptions** when
you try to use them. So using *exceptions* instead of using `null` makes nothing better at all. The reason
why we add all those `null` checks is to **prevent exceptions** to happen. So how can we get rid of that problem
if we choose to directly throw exceptions?

We can't. Throwing *exceptions* as a solution of getting rid of *null* is just a Pyrrhic victory. Not only that.
Exceptions in general share the same problems as `null`.

So what is the alternative? My Post about [null is Evil]({% post_url 2016-03-20-null-is-evil %}) contains solution
of getting rid of `null`. The thing is, the same solutions also works for *Exceptions*!
*)