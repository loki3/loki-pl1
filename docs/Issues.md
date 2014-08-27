Issues
======

Since loki3 is an experimental language, the design flaws are just as interesting as design that works well.  Here are several issues and how they could be fixed.


Eager evaluation
------------------

## What's the problem?

Pre and post arguments to a function are evaluated before they're passed to the function.  This has several implications for how programs can be written.

One issue is that you can't do any short circuiting.  For example, you would typically expect that an expression such as the following wouldn't evaluate `something`:

```
if ( false |? something )
	whatever
```

`something` may take a long time to run or it may have side effects.  In either case, you don't want it to be evaluted.


## Why is it this way?

There are several design decisions that combine to make it so eager evaluation is sometimes required:  dynamic typing, optional typing, function overloading, and a current lack of a way to annotate a function with a return type.

If you have a function that's overloaded on different types, obviously you need to know the types of the arguments before deciding which overload to call.  But you need to evaluate the expression in order to determine its type.


## How do you fix this?

First off, arguments should only be evaluated when needed.  Sometimes this will be when a function first uses it.  But sometimes this will be in order to figure out which function overload to call.

Adding function annotations to describe the return type would help cut down on the cases where eager evaluation is required.  If you wanted to completely get rid of the need for eager evaluation, you'd need to change it so a function is only allowed a single return type that is always specified.

Note: Another interesting case to consider is when a node is delimited.  You could eval the node in order to figure out if its result has higher precedence than the nodes around it and hence should be evaled first.  Or you could leave it at the default precedence and eval at the normal time.  The first option would cause issues for avoiding eager evaluation.  For example, consider the following:

```
1 + 2 * 3
1 + 2 (*) 3
```

Since `*` has higher precedence than `+`, the first line should evaluate to 7.  What about the second line?  If you first evaluate `(*)` in order to figure out that it evaluates to the multiplication function with high precedence, that line would also evaluate as 7.  But if you leave it at the default precedence without evaluating it, the `+` will be evaluated first.  Loki3 takes the second route, so this doesn't cause the same eager evaluation problem.



All arguments are evaluated
-----------------------------

## What's the problem?

Related to eager evaluation is another problem.  When any argument is used by a function, it's first evaluated.  For example, you can't do...

```
i = i + 1
```

...because the i on the left hand side gets evaluated before it's passed to the = function.  This is why it instead has to be written like this:

```
:i = i + 1
```

The = function has to take :i as a keyword to look up when it runs, since otherwise it's turned into whatever the current value of i is.


## Why is it this way?

It's a simple, consistent way to treat evaluation for all functions, whether they're prefix, postfix, infix, or delimited.

And it works nicely for pattern matching.  Consider the following:

```
[ :a :b 3 ] = [ 1 2 3 ]
[ a b 3 ] = [ 1 2 3 ]
[ a [ b 3 ] ] = [ 1 [ 2 3 ] ]
```

The first line is how you would currently use pattern matching to stuff 1 into `a` and 2 into `b`.  The second line is what you might consider "ideal" syntax.  But it needs to know to evaluate the [] delimiters (since everything's a function) without evaluating all the values inside it.  It needs to evaluate the `3` but not the `a` or `b`.  The third line demonstrates that you need to recursively evaluate delimiters without evaluating all the values inside.


## How do you fix this?

There should be a way for a function to say how it wants its arguments treated, much the way `forEachDelim` allows you to specify the delimiter used to automatically wrap around each line it parses.  So if = doesn't want its left-hand argument fully evaluated, it could instead interpret it as a keyword.  It would still have to evaluate delimiters and some types such as numbers, however.  This might be tricky to get right in practice.

This would require the addition of a "left hand expression" rule that knows when to evaluate (e.g. delimiters and numbers) versus not (e.g. keys).



Dynamic scoping
----------------

## What's the problem?

[Dynamic scoping](http://en.wikipedia.org/wiki/Scope_%28computer_science%29#Lexical_scope_vs._dynamic_scope) means that the scope where variables live is defined at runtime.  In loki3, each function call inherits the calling scope, adding to it.  When the function returns, the additional variables added by the function are removed.

The following works as expected, setting `i` to 64:

```
:i v= 2
5 repeatsOf `:i = i * 2`
```

However, the following does not work:

```
:__i v= 2
5 repeatsOf `:__i = __i * 2`
```

Why?  Because the definition of `repeatsOf` uses a local variable called `__i`.  That variable is in scope during the evaluation of `:__i = __i * 2` withing the `repeatsOf` function, stomping on the value of `__i` from the outer scope.

Another problem is that the current implementation uses chained maps, with an additional map added to the chain with each additional scope.  Values are then looked up by searching each map, starting with the deepest.  This means that a deep callstack can be slow, since all the built-in and bootstrapped functions exist in the outermost map.


## Why is it this way?

Rather than having syntax such as if-else or loops built into the language, it's defined in the language itself.  But rather than using macros to define the syntax (such as Lisp, or even the weaker form available in C), the syntax is defined as normal functions.

Now consider how you might implement an if-else using independent functions so that the following would work as expected:

```
if a?
	doSomething
else
	doSomethingElse
```

`if` is a function with a body that consists of the line `doSomething`.  `else` is a separate function with a body that consists of the line `doSomethingElse`.  The `else` block isn't associated with the `if` block in any way, yet it needs to know to run its body when the `if` block wasn't run.  Furthermore, there could be nested if-else's.  How do they coordinate their work when they're all independent function calls?

The route loki3 currently takes is for the independent functions to communicate through special variables, with dynamic scoping ensuring that nested functions operate as expected.

Now consider the earlier example using `repeatsOf`, where the code `:i = i * 2` is passed to `repeatsOf` to be evaluated multiple times.  That code needs to have the `i` that was defined on the outside in scope on the inside so that it can examine and modify it.  One way to do this is with dynamic scoping.  Of course, the drawback to this approach is that the variables used in the body of `repeatsOf` are *also* in scope.


## How do you fix this?

One possibility, of course, is to use macros for constructs such as if-else, though the language would lose something.  I specifically decided to try modeling all syntax through functions to see where it would lead.

Another possibility would be to leverage closures combined with having the variables that the functions use for communication include the stack depth so that they can properly coordinate with nested statements.  Then the language should be able to switch to lexical scoping, which is a more sane approach.

With closures, the basic idea is that whenever any code is passed off to another function, whether its a body or of *raw* type, that code is bound to the current scope rather than inheriting the scope of wherever the code is eventually called from.  Loki3 already supports manually creating closures through the `l3.bindFunction` core function and `closure` bootstrap function, so this should be fairly straight forward.
