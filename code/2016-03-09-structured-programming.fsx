(**
\---  
layout: post  
title: Structured Programming
date: 2016-03-09 00:00:00
\---
*)

(**

Back in 1968 Edsger W. Dijkstra wrote an open letter named "Go To Statement Considered Harmful". As already suggested from
the title, the very idea was to raise consciousness that the `goto` statement is more harmful then helpful. From
today view we would expect that this idea catches fire really fast, but it didn't. It took quite a long time. Even during 
all the 1970s and 1980s that question was debated a lot.

Looking back at it there are some interesting questions that is worth to look at. The first one was, if `goto` was
harmful, what was the alternative to `goto`? And why exactly is `goto` harmful, and why exactly is the alternative better?

I think that looking at this questions and their answers is still a very important step forward in understanding programming and
raise the awareness on how we achieve a more clean way of coding.

## Why "goto" was considered harmful

At first we have to understand the time in which these statement was published. It was a time in that nearly every programmer
actually used a language with such a control statement. Not only that, it usually often was the only way to provide any
kind of control structures. There were no "subroutines", no `if`, `while`, or `for` loops. No blocks of code ans so one. Sure
there existed languages that provides this kind of style of programming. The ALGOL 58 and ALGOL 60 (1958 and 1960) already
provided those kind of structures, but those languages were not used by the large. 

Most of the programmers still used languages where a `goto` statement dominated. And where the flow of a program was dominated
by direct `goto` statements. Dijkstra that was a huge proponent of the ALGOL language coined the term *Structured Programming*
as an alternative to the `goto` keyword, the idea was that we should have explicit control flow. Mainly a language should be separated
into Control Structures, Subroutines and Blocks. Control Structures should be further divided into **sequence**, **selection**, 
**iteration** and **recursion** statements. 

But why should we do something like that? The reason was that we anyway had this concepts, but implicitly. All of these ideas
was *emulated* with the `goto` statement. You wanted a `for` loop? No problem just jump with `goto` to some earlier point.

You wanted a subroutine? You pushed some values onto the stack, jumped to a certain point (your subroutine entry) and that
subroutine poped the right amount of values from a stack. That is still how functions are implemented in today's language.
That is the very reason of why you can get a `StackOverflowException` if your language don't support tail-recursion.

But by giving all of them direct labels we give all of our programs more structure. And this structure helps us in understanding
our programs. Or in other words the idea was that we should eliminate powerful construct like `goto` and yet use a lot more
weaker but specific constructs.

## Structured Programming

That overall leads to another important realization. Nearly all programming languages that we use today are basically some kind
of Structured Programming. Whether we are talking about object-oriented language, functional languages or other paradigms.

All of those languages usually provide some kind of control structures, in the form of looping `while`, `for` or `foreach`. Subroutines
in the form of procedures, functions, (static) methods or however your language names them. Selection in the form of 
`if`, `switch` or pattern matching.

It is important to realize this, because you don't really need any of those if you have a goto statement. The goto statement
could implement all of those structures just through the use of goto. That is also the reason why i said at the beginning that
Structured Programming is still a very important idea up to the current days. 

But looking more deeply into it, you will probably ask. Well is that really so? Especially if you use a functional language
you will encounter that looping constructs are discouraged. Even in object-oriented language it is more and more discouraged. 
Instead of typical `while` or `foreach` loops in C# for example you will use LINQ, Java 8 introduced Stream as a more declarative
alternative.

And yet Structured Programming is even more an important topic. The thing is. All those functional interfaces itself or LINQ or Stream
are basically Structured Programming brought to an extreme. Because the important point of Structured programming was not
the introduction of `if`, `while`, `for`, functions or blocks. The important idea was the general concept of eliminating 
something powerful and provide more declarative and specific control structures instead. 

## Powerful vs. Specific

So let's reconsider, why was `goto` harmful? `goto` was a very powerful concept. It was so powerful that we basically don't need
any concepts of looping constructs, functions and so on. The problem that it raised was that we ended up with programs that was
hard to maintain. Without **words** that describe specific problems we have problems to face growing programs and we as humans
have problems understanding what happens.

With the introduction of `if`, `while`, `for`, functions and so on we introduced specific concepts that are all possible to express
with goto. But giving them proper names we have it more easy to follow and understand the intention of our code. It is more
easier because every control structure was build for one specific task.

But when we look at the looping constructs we basically see the same things happen. Why do we consider `while`, `foreach` and so
on more and more harmful these days, and replace them with functional interfaces instead? Because those concepts are still to 
powerful. For example what does a `foreach` really express? Actually not much. It just describes a *go through all data*. That is
a general purpose way. But what can you do with it? Well a lot of powerful things!

For example you can can loop with `foreach` through an array to find the smallest element or the biggest element. You can apply a function
to every element and change every element, or insert the applied element into a new array. You can filter an array by some condition. 
You can loop through two arrays, combine them in various ways, do some calculations with every element and so on. Or in other words. 
We can do a lot with such a simple constructs. `foreach` is actually the same kind of a powerful concept like `goto`. 
And that is why we view them as *harmful*.

Looking at functional interface, they really brought the concept of Structured Programming even further. Instead of just *looping* we now
have even further separated the idea of looping. We have `List.min`, `List.max`, `List.map`, `List.filter`, `List.map2`, `List.zip`, ...
and so one just to name a view. All of those constructs are still basically just *looping* constructs. But they are specialized looping
constructs that just do one specific thing.

## So having powerful things is bad?

If we follow this thinking then it can lead to the idea that a programming language itself should not have any powerful concepts at all.
The less powerful a language is, the better. But actually it cannot be further from the truth. In fact it is quite the opposite, a language
absolute must provide as much powerful things as possible.

The reason for that is that we need those powerful concepts to build our less powerful, more specific things. And in building more of those
constructs we achieve a better more clean code. A result of this is also that code will be shorter, easier to understand with less bugs.

So we should aim for powerful constructs, and a language should provide those powerful constructs. But we should use them to build all our 
more specific less powerful constructs. Or in other words, in programming we always should prefer the least powerful concept to implement
something.

Sure you can implement filtering a `List` with a `foreach` loop. Go through every element, push it into a new `List` based on some condition.
But why not abstract such a common task? `List.filter` does exactly that. It is less powerful, because it just can filter, nothing more,  
but we only need to provide the logic for filtering. We have less code after we have a construct like `List.filter`. The intention is more clearer.
Reading a `List.filter` is easier as to basically try to understand filtering through a series of commands expressed within a `foreach`
loop. 

## Using the least powerful thing

When we follow these rule and thinking what Structured Programming is about, I think it leads to more readable code with all of
the benefit this usually have. Like higher maintainability, less bugs, in general less code because of reusable constructs. The
only thing we have todo is to identify our powerful constructs and try to come up with some alternative. In C# the usage of 
`while`, `for` or `foreach` is such a thing. In F# besides using looping constructs itself also using direct recursion or using
`fold` counts as powerful. Recursion is basically just looping, and fold is the abstraction of tail-recursive looping with
an accumulator. Watch out for them and either try to replace them with more specific constructs. Or build your own control 
structs!

## Further Reading

 * [Uncle Bob - A Little Structure](https://blog.8thlight.com/uncle-bob/2015/09/23/a-little-structure.html)

*)
