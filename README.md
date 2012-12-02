Loki
====

Loki is an experimental programming language.  Here are some of its key features:

* **Simple parse/eval rules**, with the language syntax defined entirely in the language itself
* **Easy to build DSLs**, including prefix, postfix, and infix functions, custom delimiters, and easy declarative programming
* **Pattern matching** used for assignment, multiple return values, function parameters, etc.
* **Metadata** can be attached to any value, useful for instructing the runtime, introspection, and extending the language
* Support for **higher order functions**, partial functions, and functional paradigms
* **Dynamic and static typing**, dynamic by default with explicit typing when needed


Simple Parse/Eval Rules
-----------------------

Loki supports prefix, postfix, and infix functions, i.e. functions can optionally use the value before and/or after the function itself.  Custom delimiters can be defined to succinctly call functions on lists.  Indented lines of code can be interpreted in arbitrary ways, allowing for declarative programming or the ability to define new functions that behave as if they were built in to the language.

Lisp has a simple, mathematically elegant syntax that allows for powerful extensions built on the language itself.  However, it doesn't garner much praise for its human readability (prefix functions and lots of parentheses isn't how we write anything else).  The parse and evaluation rules for Loki were inspired by the elegance of Lisp, combined with the desire to have a syntax that reads more naturally.

Parse/eval follows these steps:

* **White space:** The line is parsed into tokens based on white space
* **Delimiters:** If a token is a start delimiter, all tokens up to the end delimiter are grouped as a single node
* **Precedence:** The left-most node with the highest precedence is evaluated first
* **Consumes adjacent nodes:** Evaluating a node may call a function that consumes the nodes immediately before and/or after it, and possibly the "body" immediately following the line (i.e. the block of lines indented relative to the current line)
* **Repeat:** Evaluation continues in precedence order (left-most wins a tie), until there is no more evaluation to do

```
// defines an infix operator that uses a built-in function to create a range
:.. <- ( ->start infix ->end )
    l3.createRange { :start start :end end }
```
```
// the following assigns [ 3 4 5 6 7 ] to a
:a <- 3 .. 7
```


DSLs
----
Domain Specific Languages (DSLs) can be created in Loki.  Some of the features that make this particularly nice include the following:

* **Pre/in/postfix functions:**  For example, `5 months + 3 days` reads much easier than `(add (months 5) (days 3))` or the equivalent.
* **Custom delimiters:**  You can define your own delimiters, including functions that will be called to interpret the contents.  Imagine custom delimiters that know how to handle HTML escaping or can create your own collection behavior.
* **Function bodies:**  A body is simply the indented lines that follow the current line.  The called function can interpret those lines however it sees fit, making it easy to intuitively declare custom data structures.

Here's an example showing how you could leverage bodies for creating maps:

```
:makeMap <- func()
	:map <- nil
	:submap forEachDelim [ body :{ ]
		:map = map +a submap
	map
```
```
// :a will contain the map { :key1 42 :key2 (sqrt) }, where (sqrt) is a function
:a <- makeMap
    :key1   42
    :key2   ( sqrt )
```


Pattern Matching
----------------

Pattern matching is a common technique in functional programming languages.  It allows you to easily deconstruct a complex data structure, assigning pieces into variables, and skipping the assignment if the given value doesn't match the pattern.  In Loki, pattern matching is used for assignment, multiple return values, function parameters, function overloading (NYI), case statements (NYI), and exceptions (NYI).

```
// assign 1 into a and 2 into b
[ :a :b ] <- [ 1 2 ]
```

Here's an example that adds two complex numbers represented by maps.  The values for each key are pulled out automatically using pattern matching.

```
// the following deconstructs two incoming maps, recombining the pieces,
// effectively multiplying together two complex numbers
:* <- ( { :x ->x1 :y ->y1 } infix { :x ->x2 :y ->y2 } )
	{ :x ( x1 * x2 - y1 * y2 ) :y ( x1 * y2 + x2 * y1 ) }

// sample usage: a will be { :x -4 , :y 7 }
:a <- { :x 2 :y 3 } *c { :x 1 :y 2 }
```


Metadata
--------

Metadata can be attached to any value.  Special metadata provides additional instructions to the runtime.  Metadata allows for introspection and can be used to extend the language.

```
// @doc is used to attach documentation to values, including functions
:square <- func ->a
    a * a
:square @doc /" Function which squares its input value
```

The following example shows metadata being attached to function parameters to aid in pattern matching.

```
// function will only be called if parameter is a number
:square <- func ( ->a @@type :number )
    a * a
```

The included unit test framework allows you to attach unit tests as metadata to functions.  Combined with the ability to iterate over the defined functions in a module, this makes it trivial to ensure that every function has a unit test.


Higher Order Functions
----------------------

Functions are values just like ints or strings, meaning that they can be assigned to variables or passed to functions.  This makes it easy to apply functions to collections.  Additionally, if not all parameters are passed to a function, it evaluates to a "partial function," i.e., a new function that only requires the missing parameters to be passed.

The following shows how factorials could be computed by applying multiplication across a list:

```
:! <- ( ->n postfix )
	( 1 .. n ) fold (< #1 * #2 >)

// a will be 4*3*2*1 = 24
:a <- 4 !
```

