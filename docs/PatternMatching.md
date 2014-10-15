Pattern Matching
================

Pattern matching allows you to easily deconstruct a complex data structure, assigning pieces into variables, and skipping the assignment if the given value doesn't match the pattern.  In Loki3, pattern matching is used for the following:

* assignment
* multiple return values
* function parameters
* function overloading
* case statements (match-when)
* catching exceptions

```
// assign 1 into :a and 2 into :b
var [ :a :b ] = [ 1 2 ]

// create a function which takes two parameters
:myFunc f= func1 [ ->a ->b ]
	a + b
// function override that's called if three parameters are passed
:myFunc f= func1 [ ->a ->b ->c ]
	a + b * c

// conditional that executes the body for the first pattern match
match input
	when 1
		:result = " got a 1 "
	when [ ( ->a : :string ) ]
		:result = " got the string " + a
	default
		:result = " got something else "
```

Pattern matching works on single values, arrays, maps, and nested structures.  Metadata can be added to keys in the pattern for controlling matching.  Here are some of the additional details you can add:

* : *type*, only match if value is of given type, e.g. `( ->a : :int )`
* ..., anything left over after pattern matching is assigned to the preceding key, e.g. `( :rest ... )`
* d=, specify a default if key isn’t found, e.g. `( ->a d= 5 )`
* o=, specify that the value must be one of the items in an array, e.g. `( ->a o= [ 2 4 8 ] )`
* @@hasKeys, only match if value is a map with the given keys, e.g. `( ->a @@hasKeys [ :k1 :k2 ] )`

```
[ ( :a : :int ) ( :b : :bool ) ( :c : :string ) ] v= [ 3 false :asdf ]
// a is 3, b is false, c is :asdf
[ ( :a : :int ) ( :b : :bool ) ( :c : :string ) ] v= [ 1 2 3 ]
// mismatch

[ :a :b ( :c ... ) ] v= [ 1 2 3 4 5 6 ]
// a is 1, b is 2, and c is [ 3 4 5 6 ]

[ :a :b ( :c d= 5 ) ] v= [ 1 2 3 ]
// a is 1, b is 2, c is 3
[ :a :b ( :c d= 5 ) ] v= [ 1 2 ]
// a is 1, b is 2, c is 5

[ :a ( :b @@hasKeys [ :k1 :k2 ] ) ] v= { :a 2 :b { :k1 7 :k2 8 :k3 9 } }
// a is 2, b is { :k1 7 :k2 8 :k3 9 }
[ :a ( :b @@hasKeys [ :k1 :k2 ] ) ] v= { :a 2 :b {:k2 8 :k3 9 } }
// mismatch

( :a o= [ 3 6 9 ] ) v= 6
// a is 6
( :a o= [ 3 6 9 ] ) v= 7
// mismatch
```

How many times have you seen code like this - `drawShape(true, false)` - where you have no idea what the boolean values represent?  In loki3, you can easily create what's essentially lightweight, on-the-fly enumerated values as parameters.

```
:drawShape v= func1 [ ( ->shape o= [ :square :circle ] ) ( ->how o= [ :outline :filled ] ) ]
	if ( shape =? :square )
		drawSquare how
	else
		drawCircle how

// this draws a filled square
drawShape [ :square :filled ]

// this will fail because it doesn't match the required options
drawShape [ :circle :blue ]
```
