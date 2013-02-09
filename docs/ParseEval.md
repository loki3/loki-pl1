Parse/Eval Rules
================

Loki3 can be thought of as two languages, one built on top of the other.  The first is the core, which defines rules for parsing and evaluating expressions, with some built-in functions and almost no syntax.  The second is defined in the core language itself, in bootstrap.l3.  This is where most of the syntax is defined.  I'll refer to these as *core* and *bootstrap*.  Note that you could build a very different language on top of core, though it would still need to follow the same parse/eval rules.  Core imposes certain rules.  It's not intended to be a general purpose lexer, but it does allow a fair amount of freedom in what sort of language you build on top of it.

Here are some of the basic parse/eval rules:

1.	Tokens are space delimited
2.	Functions have an associated precedence that determines the evaluation order of tokens
3.	A function can consume the token immediately before and/or after it
4.	When a token is consumed, it's evaluated
5.	A function can consume a body, which consists of the indented lines below it
6.	Delimiters can be defined which determine how their contents are evaluated
7.	Delimiters can have associated functions that evaluate their contents

One of the goals of these rules is to make it so you can write code that feels fairly natural.  When writing out problems, we often switch between prefix (e.g. $ 5), infix (e.g.  1 + 1), postfix (e.g. 5 hours), and closed notation (e.g. { :a 2 }), so all the techniques are supported.  Here are some examples of the kinds of expressions you could write by defining the appropriate functions:

```
// 'if' is defined in bootstrap.l3, not the core language
if a =? 5
	:total = total + 1

// {} is defined in bootstrap.l3, not the core language
:myMap = { :key1 5 :key2 3 }

2 mm + 3 m + 1 ft + 1 in =>m   // add values in various units & convert to meters
```

Having all tokens be space delimited is an interesting trade-off.  It opens up more flexibility in the naming of functions, since there's no ambiguity if you use a token both as an operator and as part of another token.  For example, if . (dot) is used as a function that looks up a member, it can also be part of a function name.  The trade-off is that you need to have spaces around symbols where you wouldn't normally be required to have them, e.g. 1 + 1 rather than 1+1.
One of the unique rules is #4, that tokens are evaluated when consumed.  This means there's no distinction between l-values and r-values, as in there is in most languages.  This rule makes some things simpler to implement, but can also cause problems (which means I may change this at some point).  Consider the following:

```
a = b
```

You probably expect that to work the same as in does in most other programming languages, namely assign the value in variable b to a variable named a, but it doesn't.  = is an infix function defined in bootstrap.  When it's run, it evaluates both sides.  So if b were 5, it would eval to 5, just as you'd expect.  But it evals a as well.  If a were the string *asdf*, the = function would be handed *asdf* and 5, causing it to assign the value 5 to the variable named *asdf*.  So if you want to put a value in a variable named a, you have to use a string (representing the key in a key/value pair).  A piece of built-in syntax allows you to do this by prefixing a with :.  So you'd write it like the following:

```
:a = b
```

But, due to the flexibility of the core language, even this may not do what you expect.  b could actually be a postfix function with higher precedence than =.  In that case, b would consume =, creating a new token, which itself could be a postfix function that consumes :a.  Bootstrap defines sane functions that meet programmer expectations, but the core language has enough power that you could write very obtuse code.  Please don't.
One of the ways in which rule #4 helps is in making pattern matching simpler to implement.  The content to the left of an = could be an array or map where individual items could be nested arrays or maps and might have additional metadata describing things such as data types or default values.  The same code can deal with this structure regardless of whether it's on the left or right side of the =, or if it's used from one of the many other places where pattern matching is used (e.g. function overloading or catching exceptions) or if the pattern were attached to a variable and used later.

```
[ :a :c ] v= { :a 1 :b 2 :c 3 }
// a is now 1 and c is 3

[ :a ( :rest ... ) ] v= { :a 3 :b 4 :c 5 }
// a is now 3 and rest is { :b 4 :c 5 }

[ :a ( :b d= 4 ) ] v= { :a 6 :c 7 }
// a is now 6 and b is 4

:a v= [ :b :c ]
a v= { :b 3 :c 4 }
// b is now 3 and c is 4
```
