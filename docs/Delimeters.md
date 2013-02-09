Delimiters
==========

One of the strengths of loki3 is that it's easy to create delimiters with custom behavior.  The core language defines a few built-in ways the contents inside a pair of delimiters could be interpreted, but doesn't actually define any delimiters itself.  Specific delimiters are defined in bootstrap.l3.

Creating new delimiters
-----------------------

The core function l3.createDelimiter accepts the following options for how it interprets the contents of delimiters:

* asArray:  Turn the contents into an array
* asString:  Turn the contents into a string
* asComment:  Ignore the contents
* asRaw:  Treat the contents as raw code that could be evaluated later

You can pass an optional end delimiter.  If none is specified, the remainder of the line is considered to be the delimited contents.  The convention in bootstrap.l3 is to define a variant of each delimiter that starts . (dot) that extends to the end of the line.  This can be a convenient shorthand for expressions with lots of nested delimiters.

```
// the following two lines both define an array with three values
:a v= [ 1 2 3 ]
:b v= .[ 1 2 3
```

The final optional parameter is a function.  This allows you to process the contents however you would like.

```
// anything spanned by (+ +) will get turned into an array and passed to
// l3.addArray, effectively producing the sum of the contents
:(+ v= l3.createDelimiter { :end :+) :type :asArray :function l3.addArray }

// evals to 27
3 * (+ 2 3 4 +)
```

Bootstrap delimiters
--------------------

There are quite a few delimiters defined in bootstrap.l3.  The most basic is (), typically used to control the order of evaluation.

```
// evals to 24
2 * ( 3 + 4 )
2 * .( 3 + 4
```

[] is used to create an array.  {} is used to create a map, where the contents alternate between specifying the keys and the values.  Arrays and maps can contain values of any type.  Keys must be strings.

```
[ 1 2 3 ]
[ 1.2 :text l3.addArray ]
{ :key1 3 :key2 :something }
```

" or ' can be used to specify a string, making it easier to specify a string without having to escape " or '.  A current weakness in the language is that, since everything is space delimited, you can't specify a string that begins or ends with spaces.  """ is used for multi-line chunks of text where subsequent indented lines are considered part of the same string.  (""" is a function that takes a body.)  This is often used when attaching documentation to a function.

```
" this is a string "
' and so is this '
." and this, too

:a v= (| ## + 2 |)
:a @doc """ This defines a function that adds 2 to a value.
	The documentation for this function is longer than the function itself.
```

"[ can be used as a shorthand for converting every item in the list to a string and concatenating the results together.

```
// turns into :1text2.3
"[ 1 :text 2.3 ]

:value v= 5
print ."[ :result: \s value
// prints the string:  result: 5
```

The ignore-the-rest-of-the-line comment indicator is //.  /* */ can be used within a line to indicate a comment.  /// can be used for large block comments (analogous to """).

```
1 + 2 // this adds 1 and 2, producing 3
1 /* ignore */ + 2
/// Ignore all of this text.
	And this line.
	And this line, too.
```

There are also several pairs of delimiters that provide shortcuts for defining functions.

```
// creates a function that adds values and applies it to a specific array,
// evals to 6
(( [ ->a ->b ->c ] ` a + b + c ` )) [ 1 2 3 ]

// creates a function that multiplies together two values and applies it to
// a specific array, evals to 12
(< #1 * #2 >) [ 3 4 ]

// passes a doubling function to the apply function,
// evals to [ 6 10 14 ]
[ 3 5 7 ] apply (| ## * 2 |)
```
