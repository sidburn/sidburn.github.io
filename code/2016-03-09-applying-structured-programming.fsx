(**
\---  
layout: post  
title: Applying Structured Programming
date: 2016-03-09 01:00:00
\---
*)

(*** hide ***)
module Main

(**
In my previous post about [Structured Programming]({% post_url 2016-03-09-structured-programming %}) I 
talked about that basic looping constructs, `fold` and so on are basically just still to powerful. 
In the sense of readability we should try to eliminate them with more specific ones. In this post
i go through a *toy example* to show the various ways on how to refactor some code.

## The Toy Example

Recently I had some conversation about code in a game and providing some kind of
*critical hit-chance* in a game. The typical way on how to achieve that is actually easy. Let's
assume that every attack of an player has a 16% chance to be critical. We only need to generate
a random number between 0 and 99 (or 0 to 1) and test if that number is lower than 16 (or 0.16).

Let's assume we want to test if that really is true. We would just generate some random numbers.
Test if that number is a critical hit. And either increase a *critical hit* variable or some 
*normal hit* variable. After 1000 tries we just calculate the average and see if
we really have around 16% or around 160 hits. 

## Solution 1

Some very imperative code in F# could look like this.
*)

let rng = System.Random()

let calculateChance chance =
    let mutable criticalHit = 0
    let mutable normalHit   = 0
    for i in 1 .. 1000 do
        let random = rng.Next(0, 100)
        if random < chance then
            criticalHit <- criticalHit + 1
        else
            normalHit <- normalHit + 1
    criticalHit, normalHit

(** Testing our code would look something like this. *)

let percentage x y = 100.0 / (x + y) * x

for i in 1..10 do
    let crit,normal = calculateChance 16
    printfn "Crit: %d Normal: %d Percentage: %f" crit normal (percentage (float crit) (float normal))

(** We now getting a result like this:

    [lang=console]
    Crit: 1585 Normal: 8415 Percentage: 15.850000
    Crit: 1638 Normal: 8362 Percentage: 16.380000
    Crit: 1616 Normal: 8384 Percentage: 16.160000
    Crit: 1603 Normal: 8397 Percentage: 16.030000
    Crit: 1624 Normal: 8376 Percentage: 16.240000
    Crit: 1667 Normal: 8333 Percentage: 16.670000
    Crit: 1617 Normal: 8383 Percentage: 16.170000
    Crit: 1639 Normal: 8361 Percentage: 16.390000
    Crit: 1653 Normal: 8347 Percentage: 16.530000
    Crit: 1613 Normal: 8387 Percentage: 16.130000

Actually we now have proven that the idea works, as around 16% of our attacks are now critical. 
But now let's actually look if we can improve our `calculateChance` function. As stated in the
beginning. It is a toy example, so we usually wouldn't waste any time in improving this particular
function. But by going through a toy example it can help to get the general concept on
how to improve code.

## Solution 2

In functional programming mutable variables are actually a little bit frowned upon. So let's try
to eliminate our mutable variables by replacing our loop with recursion. Actually recursion is in that
sense just looping and you can turn any kind of loop into recursion. The difference between looping
and recursion is just what is explicit and what is implicit.

In Looping we implicitly move to the next step, and we can explicitly break/abort a loop. In recursion
we implicitly abort, and we have to explicitly move to the next step, by calling our function recursively. 

Mutable variables outside of a loop turn into function parameters. This way we can turn any kind of
loop into a recursive function and eliminate mutable variables all together.
*)

let calculateChance chance =
    let rec loop count criticalHit normalHit =
        if count < 1000 then
            let random = rng.Next(0,100)
            match random < chance  with
            | false -> loop (count+1) criticalHit (normalHit+1)
            | true  -> loop (count+1) (criticalHit+1) normalHit
        else
            criticalHit, normalHit
    loop 0 0 0

(**
We now eliminated all mutable variables and now provided an inner recursive function that replaces
our loop. On top of it i replaced the inner `if` check on `random < chance` with pattern-matching.
This way it is easier to see the difference.

Either in both ways we will call `loop` again, but with an increment `count`. If `random < change`
is `false` we have a normal hit, so we increase `normalHit` by one, otherwise we increase `criticalHit`
by one.

We continue our recursive call as long we have `count < 1000`. But as soon that condition is `false` 
we end up with just `criticalHit, normalHit` that will return both variables as a tuple.

The question overall is. Is that version better as *Solution 1*?

Well it depends. We eliminated the mutable variables, but actually at least I am someone that
has nothing against mutable variables in a limited scope. If you are a programmer that primarily 
uses an imperative language and are used to looping then you will probably prefer Solution 1. If you
are in the state of learning *functional programing* you should try to replace looping
constructs in this kind of way to get used to it. This is especially important for the later Solutions
we will look at. If you are used to this, like me, you will probably not find the recursive version
any harder to understand as the looping example.

So what is with Structured Programming? Did we replace a powerful construct with a less powerful
construct? At that moment, no we didn't. Recursion is just as much powerful as looping. The funny
part. The compiler will turn such kind of tail-recursion into a loop when compiled. That's also
the reason why functional programmers names such inner recursive functions just `loop`.

So was our transformation into *Solution 2* wasteful? Not really, that now leads to *Solution 3*

## Solution 3

Once we have eliminated all kinds of mutable variables we end up with a recursive function that
just loops. Our function takes some additional variables like `count`, `criticalHit` and `normalHit`
as it's state. What we really have is a looping construct with just an accumulator. But wait. That's
exactly what `fold` is about! The question that starts to beg is. Can we replace *Solution 2* somehow
by using a `fold`?

The answer is *yes*. But which kind of `fold` do we need? Or in other words, over what exactly do
we loop? We don't loop over a data-structure like an `Array` or `List`. So what is it that 
we are looping over?

When we examine our code we could say we loop over `count`. But that isn't true. Our
`count` is just there so we know when to end the looping. We really are looping over the
`rng.Next(0,100)` call. We only need the `count` because that is an infinite sequence.
But that actually answers our question. Let's create a `seq` that just returns an
infinite sequence of random numbers.
*)

let rng = System.Random()
let randomSeq min max = Seq.initInfinite (fun _ -> rng.Next(min, max))

(**
Note that I'm defining `rng` outside the function. Instantiating a `System.Random` inside the function
and calling `randomSeq` in short time-interval would otherwise lead to a RNG with the same seed.
Once we have our Random Sequence it also becomes clearer what our `count` was before. We just `take`
the amount of randoms we need from our infinite sequence. After that, we only need to transform what is
left into a `fold` call.
*)

let calculateChance chance =
    randomSeq 0 100
    |> Seq.take 1000
    |> Seq.fold (fun (criticalHit,normalHit) random ->
        if   random < chance 
        then (criticalHit+1),normalHit
        else criticalHit,(normalHit+1)
    ) (0,0)

(**
Looking at the current Solution i think we made our first improvements to our code. At first we
created a `randomSeq`. A sequence out of random can be helpful in many other places. `Seq.take 1000`
makes it clear that we just fetch `1000` random numbers from it. And after having those we use `fold`.

Now, our `fold` only contains the real logic. It just checks whether our random is a criticalHit or not
and we create our new state from it.

But as stated before. `fold` is still something that we consider as *powerful*, is there a way to also
eliminate `fold`?

## Solution 4

We actually can eliminate `fold`. But for that we need to go and look back at what we are actually 
doing. What we really was interested in was the percentage if we really get the right amount of
*critical-hits* this way. What we really need is to split it into two parts. We need some function 
that test whether we have a critical hit or not. We could turn it into `true` and `false` values.
But later we need to turn those somehow into a formula to calculate the average.

But by thinking separetely about it we easily can recognise that we can easily achive both things in
one step. By just turning a critical-hit into `1.0` and a normal hit into `0.0`. By calculating the
average we would automatically get a percentage that ranges bewteen `0.0` and `1.0`. We could multiply it by 
`100.0` or we also could use `100.0` and `0.0` instead of `1.0` and `0.0`.
*)

let calculateChance chance =
    randomSeq 0 100
    |> Seq.take 1000
    |> Seq.map (fun x -> if x < chance then 100.0 else 0.0)
    |> Seq.average

(** And as a final note. We can replace a call to `map` followed by `average` with `averageBy` *)

let calculateChance chance =
    randomSeq 0 100
    |> Seq.take 1000
    |> Seq.averageBy (fun x -> if x < chance then 100.0 else 0.0)

(** 
## Conclusion

When we compare the final code with what we started I think we have reached a good refactoring. 

We started with:
*)

let rng = System.Random()

let calculateChance chance =
    let mutable criticalHit = 0
    let mutable normalHit   = 0
    for i in 1 .. 1000 do
        let random = rng.Next(0, 100)
        if random < chance then
            criticalHit <- criticalHit + 1
        else
            normalHit <- normalHit + 1
    criticalHit, normalHit

let percentage x y = 100.0 / (x + y) * x

for i in 1..10 do
    let crit,normal = calculateChance 16
    printfn "Crit: %d Normal: %d Percentage: %f" crit normal (percentage (float crit) (float normal))

(** End we ended with *)

let rng = System.Random()
let randomSeq min max = Seq.initInfinite (fun _ -> rng.Next(min, max))

let calculateChance chance =
    randomSeq 0 100
    |> Seq.take 1000
    |> Seq.averageBy (fun x -> if x < chance then 100.0 else 0.0)

for i in 1..10 do
    let percentage = calculateChance 16
    printfn "Percentage: %f" percentage

(**
From a code perspective we didn't write more code. We even reduced the line count number.
By rewriting we created a reusable `randomSeq` sequence that can provide
us an infinite stream of random numbers. I also find the logic easier to understand.

1. Initialize a `randomSeq` that return numbers from 0 to 99
2. Take 1000 of those numbers
3. Map them to `100.0` if it smaller than `chance` or `0.0` and calculate the average from those numbers.

As stated in the beginning. It was a toy program with what we started but overall we can see the ways
in how we can continuously improve our code.

I don't think that just turning a loop in itself into
a recursive function as you see in *Solution 2* provides much benefit. But doing such a thing
can help to later turn it into a `fold` call. You also can move directly from *Solution 1* to
*Solution 3*. But doing the intermediate Step can greatly help if you are not used in doing this kind
of things.

Once you have a `fold` in it you can further think in how you can eliminate it. If there
exists other functions that can replace a `fold` use them instead! But what happens if you don't
find other more specific function as `fold`? Well just use it, it is there to be used. But if you
found similarities between multiple different lambda functions that you pass into `fold`, you should
look into how you can abstract the lambda into a reusable function.
*)
