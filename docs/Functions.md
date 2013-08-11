Functions
=========

Functions are values just like ints or strings, meaning that they can be assigned to variables, passed to functions, or generated based on data.  This makes it easy to apply functions to collections, for example.  Additionally, if not all parameters are passed to a function, it evaluates to a "partial function," i.e., a new function that only requires the missing parameters to be passed.  Metadata can be attached to a function to provide documentation or to change how the run-time deals with the function.  Functions can also be bound to a scope, allowing such features as closures.
The last value in a function body is what the function evaluates to (this is no explicit return).  During the eval phase, functions can consume the token before it and/or after it in an expression.  Additionally, it can consume a function body, which can be specified by indenting the lines following the function invocation.

Prefix, postfix, and infix
--------------------------

The function names used for defining functions contain hints as to where it the new function will find its parameters.  A 1 can appear at the front and/or end of *func* to indicate it expects a parameter before and/or after the function name.  A 0 indicates the function takes no parameters.

Here's an example of defining a prefix function:

```
:add2 v= func1 ->b
	b + 2
// the following evaluates to 7
add2 5
```

Here's a postfix function:

```
:plus2 v= ( ->b 1func )
	b + 2
// the following evaluates to 5
3 plus2
```

Here's an infix function:

```
:plus v= ( ->b 1func1 ->c )
	b + c
// the following evaluates to 42
40 plus 2
```

Here's a function that takes no parameters:

```
:help v= func0
	print ." This could be a helpful explanation, but it's not.
```

In order to indicate that a function should consume a body, metadata is attached to the function using @body?.  Here's an example of a function that accepts a body:

```
:repeat v= func1 ->n
	:i v= 0
	l3.loop { :check ` i <? n ` :change ` :i = i + 1 ` :body body }
:repeat @body? true

// the following repeats a body of code
:product v= 1
repeat 4
	:product = product * 2
// total is now 16
```

Pattern matching parameters
---------------------------

Function parameters can be specified using pattern matching.  The values before or after the function in an expression could be an array, providing a way to pass multiple parameters, or a map, providing a way to pass named parameters.  Pattern matching can be used to extract out portions of the array or map.

```
:sum v= func1 [ ->a ->b ]
	a + b
// the following evals to 5
sum [ 2 3 ]

:process v= func1 { :add ->a :mult1 ->m1 :mult2 ->m2 }
	a + m1 * m2
// the following evals to 11
process { :mult1 2 :mult2 3 :add 5 }
```

Pattern matching with defaults and substructures allow you to have optional and named parameters.

```
// optional parameter
:doStuff v= func1 [ ->a ( ->b d= 2 ) ]
	a + b
// evals to 6
doStuff [ 1 5 ]
// evals to 3, since b takes the default
doStuff [ 1 ]

// optional named parameters
:process v= func1 { :add ->a :mult1 ( ->m1 d= 2 ) :mult2 ( ->m2 d= 3 ) }
	a + m1 * m2
// the following evals to 11
process { :add 5 }
// the following evals to 17
process { :add 5 :mult1 4 }
```

Pattern matching also allows you to optionally restrict parameters to the specified type.

```
:sum v= func1 [ ( ->a : :number ) ( ->b : :number ) ]
	a + b
// evals to 5
sum [ 1.5 3.5 ]
// the following types don't match 'sum', so the function won't be called
sum [ 2 :text ]
```

You can do function overloading, where the system calls the function that's the most specific match.  Note that this requires using f= instead of v= for assigning the function definition into a variable to avoid simply overwriting the previous value associated with the function name.

```
// if ints are passed, add them
:+ f= .( ( ->a : :int ) 1func1 ( ->b : :int )
	l3.add .[ a b

// turn contents into strings and concatenate them
:+ f= .( ->a 1func1 ->b
	l3.stringConcat .{ :array .[ ( $ a ) ( $ b )

// evals to 7
3 + 4
// evals to :3text
3 + :text
```

You can do structural typing, where a function is only called if the parameter has the specified keys.  This technique can be used as a dynamic, yet type-safe, way to enforce that an object exposes certain functions or values.

```
:step v= func1 ( ->thing @@hasKeys [ :hasNext :next ] )
	:result v= 5
	if thing . :hasNext
		:result = thing . :next
	result

// evals to 6
step { :hasNext true :next 6 }
// evals to 5
step { :hasNext false :next 6 }
// doesn't match, so an exception is thrown
step { :hasNext true }
```

Simple declarations
-------------------

Since it's simple to extend the syntax of loki3, multiple ways of defining functions are provided that are convenient in different situations.  Short prefix functions with an arbitrary number of named arguments can be declared using (( )).

```
// (( )) creates a prefix function.  we use it to declare a function with three arguments,
// then an array is passed to it, so this expression evals to 30
(( [ ->a ->b ->c ] ` a * b * c ` )) [ 2 3 5 ]
```

Functions taking exactly one argument can be declared using (| |).  The implicit parameter is referenced as ##.  This is useful for passing simple functions to other functions such as apply, which take a function as a parameter.  The following example doubles every value in an array:

```
// evals to [ 6 10 14 ]
[ 3 5 7 ] apply (| ## * 2 |)
```

Functions taking exactly two arguments can be declared using (< >).  The implicit parameters are referenced as #1 and #2.  This is useful for passing simple functions to functions such as fold.  The following example multiplies together every value in an array:

```
// evals to 30
[ 2 3 5 ] fold (< #1 * #2 >)
```

To remember the difference between (| |) and (< >), you just need to think about the fact that | is a single line (and hence (| |) takes a single argument) and < is two lines (and hence (< >) takes two arguments).  Parentheses can be drawn with an arbitrary number of line segments, corresponding to (( )), which can take an arbitrary number of arguments.

Partial functions
-----------------

As is common with functional languages, passing a subset of the required arguments to a function will return a new function that's bound to the passed arguments and requires the missing arguments.

```
:doStuff v= func1 [ ->x ->y ]
	x + y

// creates a partial function where x is 2 and y still needs to be passed
:add2 v= doStuff [ 2 ]
// evals to 5
add2 [ 3 ]
```

But, unlike most functional languages, you have more freedom than simply creating partial functions based on the order parameters are passed to the function.  If the function takes a map, it can bind to any of the named parameters.

```
:doStuff v= func1 { :x ->x :y ->y }
	2 * x + 3 * y

// creates a partial function bound to x=2
:a v= doStuff { :x 2 }
// evals to 16
a { :y 4 }

// creates a partial function bound to y
:b v= doStuff { :y 2 }
// evals to 14
b { :x 4 }
```

Active functions
----------------

One thing that can be tricky when dealing with functions is the fact that they get actively evaluated when appearing in an expression.  The following code defines a postfix function, then attempts to assign it into another variable.  But instead, the interpreter ends up trying to evaluate the function, since it has higher precedence than v=, causing an error.  You can surround the function with ( ) to keep parse/eval from eagerly evaluating the function.

```
:! v= ( ->n 1func )
	( n <? 1 ) ? .[ 1 .` ( 1 .. n ) fold (< #1 * #2 >)
// evals to 120
5 !

// this throws an exception since it tries to eval ! taking v= as a parameter
:factorial v= !
// this is how you could assign the ! function into :factorial
:factorial v= ( ! )
// now this will eval to 120
5 factorial
```

This works great as long as the function requires parameters so the function can't be fully evaluated.  But if the function doesn't take any parameters, it will still evaluate the function even when wrapped in ( ).

Function metadata
-----------------

As with any value, additional metadata can be attached to a function.  Typically this is done by using the key (the name it's stored under) rather than the function value itself to avoid the "active functions" issue mentioned above.  Here are some of the uses for metadata attached to functions:
* @doc:  Provide documentation on how to use the function
* @cat:  List the category it's part of
* @order:  Set the evaluation precedence
* @body?:  Declare that the function requires a body
* @unittest:  Associate a unit test with the function

Since each of these metadata functions return the key, they can easily be chained together.

```
:triple v= func1 ( ->x : :number )
	3 * x
:triple @cat :math @doc ." Triple a number
```

When figuring out the value of an expression, it evaluates the left-most function with the highest precedence first.  The default precedence for evaluating a function is 3.  Lower numbers cause the function to be evaluated earlier.  Higher numbers make the function get evaluated later.

```
:plus v= ( ->a 1func1 ->b )
	a + b
:times v= ( ->a 1func1 ->b )
	a * b

// evals to 9, since the left-most function is evaluated first
1 plus 2 times 3

// increase the precedence of times
:times @order 2

// now this evals to 7, since times is evaluated before plus
1 plus 2 times 3
```

Binding to a scope
------------------

Currently the bootstrapped language doesn't create closures automatically.  You have to manually create them.  Creating a closure, i.e., binding a function to a scope, allows a function to operate on its own local idea of the world.

```
:value v= 1

// not using a closure
// returns a function that adds onto the global 'value'
:nested v= func1 ->value
	:add v= func1 ->plus
		value + plus
	add
// 2 gets lost, instead it'll add onto 1...
:getValue v= nested 2
// ...hence this evals to 4
getValue 3

// creates a closure bound to the passed value
:nested2 v= func1 ->value
	:add v= func1 ->plus
		value + plus
	closure :add
// this time it'll add onto 2...
:getValue v= nested2 2
// ...so this evals to 5
getValue 3
```
