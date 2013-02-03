Loki3
=====

Loki3 is an experimental programming language.  Here are some of its key features:

* **Simple parse/eval rules**, with the language syntax defined entirely in the language itself
* **Easy to build DSLs**, including prefix, postfix, and infix functions, custom delimiters, and easy declarative programming
* **Pattern matching** used for assignment, multiple return values, function parameters with overloading, catching exceptions, etc.
* **Metadata** can be attached to any value, useful for instructing the runtime, introspection, and extending the language
* Support for **higher order functions**, partial functions, and functional paradigms
* **Dynamic and static typing**, dynamic by default with explicit typing when useful


Simple Parse/Eval Rules
-----------------------

Loki3 supports prefix, postfix, and infix functions, i.e. functions can optionally use the value before and/or after the function itself.  Custom delimiters can be defined to succinctly call functions on lists.  Indented lines of code can be interpreted in arbitrary ways, allowing for declarative programming or the ability to define new functions that behave as if they were built in to the language.

Lisp has a simple, mathematically elegant syntax and the concept of data is code and code is data, allowing for powerful extensions built in the language itself.  However, not everyone is fond of prefix functions and lots of parentheses, since that isn't how we write anything else.  The parse and evaluation rules for Loki3 were inspired by the elegance of Lisp, combined with the desire to have a syntax that reads more naturally.

Parse/eval follows these steps:

* **White space:** The line is parsed into tokens based on white space
* **Delimiters:** If a token is a start delimiter, all tokens up to the end delimiter are grouped as a single node
* **Precedence:** The left-most node with the highest precedence is evaluated first
* **Consumes adjacent nodes:** Evaluating a node may call a function that consumes the nodes immediately before and/or after it, and possibly the "body" immediately following the line (i.e. the block of lines indented relative to the current line)
* **Repeat:** Evaluation continues in precedence order (left-most wins a tie), until there is no more evaluation to do

The following shows a sample line, which gets tokenized based on white space.  *a* and *c* are both lists that get interpreted based on custom code.  *b*, *d*, and *f* are infix functions which consume the nodes on either side of them.  Since *f* has highest precedence, it's evaluated first.  _3 * 2_ gets replaced by 6.  Next, *b* and *d* have equal precedence, so *b*, the left-most, is evaluated first, consuming *a* and *c* in the process.  Then *d* is applied, using the previous results from both sides to compute the final value.

```
( 1 .. 4 ) fold (< #1 * #2 + #2 >) + 3 * 2
|        |  |   |                | | | | |
 --------   |    ----------------  | | | |
     |      |            |         | | | |
     a      b            c         d e f g
```

The language defines these parse/eval rules and combines this with a set of built-in functions, with the language syntax "bootstrapped" from there.Even functionality such as assignment, if statements, and function definitions are bootstrapped, including every function and delimiter in the above example (*, +, () (||)).  This makes the entire syntax very flexible.


DSLs
----
Domain Specific Languages (DSLs) can be created in Loki3.  Some of the features that make this particularly nice include the following:

* **Pre/in/postfix functions:**  For example, `5 months + 3 days` reads much easier than `(add (months 5) (days 3))`, `Dates(0,5,0).Add(Date(0,0,3));` or the equivalent.
* **Custom delimiters:**  You can define your own delimiters, including functions that will be called to interpret the contents.  Imagine custom delimiters that know how to handle HTML escaping or can create your own collection type.
* **Function bodies:**  A body is simply the indented lines that follow the current line.  The called function can interpret those lines however it sees fit, making it easy to intuitively declare custom data structures.

```
// define the .. infix operator that uses a built-in function to create a range
var :.. = ( ->start infix ->end )
    l3.createRange { :start start :end end }

// the following assigns [ 3 4 5 6 7 ] to :a
var :a = 3 .. 7
```
Here's an example showing how you could leverage bodies for creating maps:

```
// defines a function that takes a body and turns it into a map
var :makeMap = func()
	var :map = nil
	:submap forEachDelim [ body :{ ]
		:map = map + submap
	map

// :a will contain the map { :key1 42 :key2 (sqrt) }, where (sqrt) is a function
var :a = makeMap
    :key1   42
    :key2   ( sqrt )
```


Pattern Matching
----------------

Pattern matching is a common technique in functional programming languages.  It allows you to easily deconstruct a complex data structure, assigning pieces into variables, and skipping the assignment if the given value doesn't match the pattern.  In Loki3, pattern matching is used for assignment, multiple return values, function parameters, function overloading, case statements (match-when), and catching exceptions.

```
// assign 1 into :a and 2 into :b
var [ :a :b ] = [ 1 2 ]
```

Here's an example that adds two complex numbers represented by maps.  The values for each key are pulled out automatically using pattern matching.

```
// the following deconstructs two incoming maps, recombining the pieces,
// effectively multiplying together two complex numbers
var :* = ( { :x ->x1 :y ->y1 } infix { :x ->x2 :y ->y2 } )
	{ :x ( x1 * x2 - y1 * y2 ) :y ( x1 * y2 + x2 * y1 ) }

// sample usage: :a will be { :x -4 , :y 7 }
var :a = { :x 2 :y 3 } * { :x 1 :y 2 }
```


Metadata
--------

Metadata can be attached to any value.  Special metadata provides additional instructions to the runtime.  Metadata allows for introspection and can be used to extend the language without adding syntax to the core language.

```
// @doc is used to attach documentation to values, including functions
var :square = func ->a
    a * a
:square @doc ." Compute the square of a number
```

The following example shows metadata being attached to function parameters to aid in pattern matching.  Note that : is a function that adds metadata to a key describing the data type to use during pattern matching.

```
// function will only be called if parameter is a number
var :square = func ( ->a : :number )
    a * a
```

The included unit test framework allows you to attach unit tests as metadata to functions.  Combined with the ability to iterate over the defined functions in a module, this makes it trivial to ensure that every function has a unit test.


Higher Order Functions
----------------------

Functions are values just like ints or strings, meaning that they can be assigned to variables, passed to functions, or generated based on data.  This makes it easy to apply functions to collections, for example.  Additionally, if not all parameters are passed to a function, it evaluates to a "partial function," i.e., a new function that only requires the missing parameters to be passed.  Functions can also be bound to a scope, allowing such features as closures.

The following shows how factorials could be computed by applying multiplication across a list:

```
// defines a postfix function that takes a single parameter
var :! = ( ->n postfix )
	( 1 .. n ) fold (< #1 * #2 >)

// uses the previous function definition
// :a will be 4*3*2*1 = 24
var :a = 4 !
```
