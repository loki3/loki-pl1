Debugging
=========

As with many languages, one simple way to figure what your program thinks it's doing is to add print statements.  One technique for building up strings is to use the $ function, which converts the next token to a string.  You can then combine this with concatenation to print out details:

```
print " current contents of x: " + $ x + " and y: " + $ y
```

Another way is to fire up a copy of the REPL from within your code so you can examine and/or modify the state of the current scope.

```
:myFunc f= func1 ->a
	if a
	    repl
```

Or, if you can't figure it out in loki3, you can try breaking into the debugger for the interpreter by using *l3.debug.break*.
