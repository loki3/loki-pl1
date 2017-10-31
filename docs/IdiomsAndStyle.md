Idioms and Style in loki3
=========================

Express the domain naturally
----------------------------

You can leverage loki3's ability to make functions that are prefix, postfix, infix, delimited, and/or with bodies in order to express the domain more naturally.
Sketch on a piece of paper how you would like to express the problem, then try to shape the code so it looks that way.

For example, if you wanted to add together linear units and convert to a unit of your choice, you might sketch out something like this:

```C
5 m + 3 in =>ft
```

And to declare all the necessary functions and conversion factors for any sort of units, you'd want to make it as simple as possible to declare everything,
perhaps something like the following, so it would be easy to add different sets of units and measurements:

```C
defineUnits [:meters unitsType.distance]
	:mm  :millimeters 0.001
	:m   :meters      1
	:in  :inches      0.0254
	:ft  :feet        (0.0254 * 12)
```

If you look at [test.l3](../l3/test.l3) (for implementation) and [test_tests.l3](../l3/test_tests.l3) (for sample usage), you can see how the above takes just a few lines of code to describe in loki3.
In most other languages, you would have a couple options:
either put a lot of work into a grammar, lexer, parser, etc. in order to build an external language that has friendly syntax,
or accept the limited syntax of the host language to create something much less readable.

```C
// in a curly brace language, you might have this
Add(new Meters(5), new Inches(3)).ToFeet();

// in a Lisp, you might have this
(to-feet (add (meters 5) (inches 3)))

// contrast with how you might scribble it on paper
5 m + 3 in =>ft
```

There are also some languages that allow a limited amount of syntax changes through macros, essentially a meta-language that can manipulate the syntax of the language.
It's interesting to note that loki3 accomplishes its syntax tricks without a meta-language, primarily through the fact that all tokens are whitespace delimited combined with its flexible parse/eval rules.
This allows a wide range of naming that can include numbers and symbols, similar to, say, Clojure, but with more options for syntax, e.g. infix and postfix functions.


Infix for dataflow / Functions over loops
-----------------------------------------

Often, code is about transforming one thing into another.
In loki3, you would usually use an infix function, with the input on the left and the output on the right.
This allows you to easily chain together multiple transforms, with the output of one feeding in as the input to the next one.
The bootstrapped syntax leverages this pattern from many of its basic function definitions.

```C
// member access is infix
{ :a 1 :b { :c 3 } } . :b . :c
// returns 3

// adding metadata is infix to enable a fluent style
:myfunc @cat :mycat @doc "Useful documentation"

// chaining array manipulation to double all the odd numbers from 1 to 10 and add the results
// this leverges the following infix functions: .., filter, apply, fold
1 .. 10 filter (| ## % 2 =? 1 |) apply (| ## * 2 |) fold (+)

// alternate syntax for above, where previous output is fed in as input to each new line
1 .. 10 pipe
		filter (| ## % 2 =? 1 |)
		apply (| ## * 2 |)
		fold (+)
```

While loki3 has a rich set of loops, it's generally preferred to use functions that operate on collections, such as apply, filter, and fold.
Loops require side effects to perform their operations, so they're generally avoided when reasonable.


Use maps for data structures, keys for types
--------------------------------------------

Rich data is typically passed around in a map.
Functions check for the presence of keys to operate on the data in the map (structural typing).

A simple example can be seen in [wrap.l3](../l3/wrap.l3), which defines an *optional* type and functions to operate on them.
The concept of an optional value being present is captured by a map with a *some* key, `{ :some value }`.
The concept of no value is captured by a map with a *none* key, `{ :none nil }`.
Code can decide which path to take based on the presence of those keys, with convenience functions making it easier to check.
Here's an abbreviated version of the definitions and sample usage:

```C
// definitions
:some v= (| { :some ## } |)
:none v= { :none nil }
:isNone v= ( @@hasKeys [ :none ] )

// use
:f v= multifunc1
	when { :some ->x }
		print "got a value: " + $ x
	when ->x isNone
		print "no value"
```

Note that this code uses `{ :some ->x }` rather than `->x isSome` so that it can extract the value out of the map.

As another example, you might keep various game stats in a map, pulling out values associated with different keys depending on what you need.

```C
:canEnterGreatRoom v= func1 ->player
	( player . :health >? 500 ) &? ( player . :magic >? 50 )

canEnterGreatRoom { :name "Gwydryn" :health 512 :class "bard" :magic 42 }
// returns false
```

Even the *public* function, which can be used to create classes, uses a map to store its information.

```C
// define a class with a private variable and three member functions
:tracker v= func1 ->initial
	:total    v= initial

	:add      v= (| :total = total + ## |)
	:subtract v= (| :total = total - ## |)
	:current  v= (- total -)

	public
		:add
		:subtract
		:current

// create an instance of the class intialized with the value 42
:a v= tracker 42
// a is now { :add <func> :subtract <func> :current <func> }
a . :add 3
// returns 45
a . :current
// returns 45

// this function takes any map with :add and :current
:doubleIt v= func1 ( ->adder @@hasKeys [ :add :current ] )
	adder . :add (adder . :current)

doubleIt a
// returns 90
```


Dynamic versus types
--------------------

Loki3 is a dynamic scripting language with optional types.
Take advantage of dynamic typing for quick and dirty scripting.
Add types for better documentation, more precise specification, multiple dispatch (or function overloading), and pattern matching.

Named parameters for readability and rich APIs
--------------

Named parameters can make code more readable.
Combine with default parameters, "one of" (described below), and partial application, to create rich APIs.


Style
=====

Use trailing ? for boolean functions and variables
--------------
This is used to distinguish between assignment (=) and the equality check (=?), for example.
The variable should be named such that it forms a natural question, e.g. `change?` answers the question of whether you should change something.

Prefer "one of" for boolean function parameters
------------
If a function takes true or false and the meaning would be ambiguous at the caller's site, use "one of" instead.
This is a lightweight syntax for making the two options explicit, e.g. `->shape o= [ :square :circle ]` instead of `isSquare? : :bool`.
In the following example, booleans are used to decide whether to draw a square or circle and if the shape should be outlined or filled.
But you can't understand the `drawShape` call without looking at the function definition.
And even within `drawShape` you can't tell what will happen if `outline?` is false.

```C
:drawShape v= func1 [ ->square? ->outline? ]
    if square?
        drawSquare outline?
    else
        drawCircle outline?

// what does this line do?
drawShape [ true false ]
```

"One of" makes the code much more obvious to read and understand, yet it's lightweight enough that it adds very little additional ceremony.

```C
:drawShape v= func1 [ ( ->shape o= [ :square :circle ] ) ( ->how o= [ :outline :filled ] ) ]
    if ( shape =? :square )
        drawSquare how
    else
        drawCircle how

// this line is self-documenting
drawShape [ :square :filled ]
```

Function documentation
----------------------
Every top level function should generally have an associated category (typically shared with the rest of the file) and a short explanation of what it does.
The category and documentation can live on the same line.
Note that this immediately follows the function declaration, since the key doesn't exist until the function has been declared.

```C
:double v= func1 ->x
	x * 2
:double @cat :mathStuff @doc "Double the given number"
```

If the use of the function is at all ambiguous, an example can be provided.
If the type of the parameters is ambiguous, it's preferable to add explicit typing instead of dynamic typing.

```C
:double v= func1 (->x : :number)
	x * 2
:double @cat :mathStuff @doc "Double the given number"
:double @example "double 5.2"
```


Tests
-----
It's highly recommended to have a unittest for everything in an implementation file.
The unittest framework is set up so that it's easy to validate that every token has an associated unittest that passes as well as associated documentation.
The associated tests are typically put in a file with the same name as the implementation file, but ending in _tests.

```C
// foo.l3
:double v= (| ## * 2 |)

// foo_tests.l3
:double @unittest func0
	6 assert=? double 3
```

As a convenience from the REPL, you could also include a function in the implementation file that runs the associated unittests.
The name would be `testFoo` where `Foo` is the category the functions live in (e.g. `:token @cat :foo`).

```C
:testFoo v= func0
	import "unittest.l3"
	unittest [ "foo.l3" "foo_tests.l3" ]
```

Functions without tests can either use `@unittest testNYI` if the test hasn't been written yet ("not yet implemented") or `@unittest noTestNeeded` if a test wouldn't be useful.
To avoid having to document and test functions that are only used locally by a single function, such functions are often made local to the top level function that needs them.


Miscellaneous naming conventions
------------------------

* A function that converts something to a foo is called `=>foo`
* Variations on setting a value end with `=`, e.g. `v=` for declaring and setting a variable, `d=` for setting a default value
* Delimiters generally contain one or more of `([{</|\`, perhaps prefixed with letters and ending with a matching delimiter, e.g. `(| # * 2 |)` or `r[ 1 2 ]r`
* Delimiters that use the remainder of the line use the same name as regular delimiters but start with a period, e.g. `"string"` versus `." string`
* Functions for setting an item's metadata start with `@`, e.g. `@doc` for attaching documentation to a token
* Functions for getting an item's metadata start with `.@`, e.g. `.@doc` for fetching a token's documentation


Quirks
------
`var :a = 5` declares a token named :a and assigns the value 5 to it.
This style looks most similar to other languages, so it's often used in examples.
`:a v= 5` is shorthand for the same thing and is typically used in actual code.

`:token` was originally intended to be like atoms in Clojure.
But tokens are just represented as strings, so instead this syntax ended up being used for strings that don't contain any whitespace.
As a result, many short strings use the colon as a prefix rather than the double quote delimiters.

Sample code generally uses start/end delimiters for wrapping values and functions.
But, as a quick way to add delimiters that span to the end of the line, you can use the `.delim` alternative.
Sometimes there can be a whole series of end-of-line delimiters used simply to make typing easier.
This can make it easier to add new content to the end of delimited values because it's simpler to go the end of the line in a text editor.
For example, the following line appears in help.l3:

```C
:overload v= .{ :pre  .( getParams .( key .@ :l3.func.previous
```

This could have been written like this instead:

```C
:overload v= { :pre  ( getParams ( key .@ :l3.func.previous ) ) }
```
