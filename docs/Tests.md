Tests
=====

Loki3 comes with a simple test framework written in the language itself.  You can run a test for every function in a file or scope, run an arbitrary test suite, or test that all functions have documentation.
You can attach a unit test to a function.  This makes it simple to ensure that every function you write has an associated unit test.  If it doesn’t, the unit tests will fail.  Likewise, you can ensure that you’ve provided at least minimal documentation for every function.
Say that stuff.l3 consists of the following functions:

```
:double v= func ->a
	a * 2
:double @doc ." Double a number

:triple v= func ->a
	a * 3
:triple @doc ." Triple a number

:quadruple v= func ->a
	a * 4
:quadruple @doc ." Quadruple a number

:printHelp v= func()
	print ." You can double or triple a number
```

You could then create a file called stuff_tests.l3 containing the unit tests for each of those functions.

```
:double @unittest func()
	2 assert=? double 1
	5 assert=? double 2.5

:triple @unittest func()
	6 assert=? double 2

// indicates that we still need to write this test
:quadruple @unittest testNYI

// indicates that we don't need a unit test for this function
:printHelp @unittest noTestNeeded
```

You could run these tests using the following lines:

```
import :l3/unittest.l3
unittest [ :stuff.l3 :stuff_tests.l3 ]
```

This would print out diagnostics if any of the tests failed or weren’t present or if any function didn’t have @doc metadata attached to it.  Alternately, you could use runTestFile :myfile.l3 to run every function in a file if you had a suite of tests that didn’t directly correspond to functions.
There are several basic asserts included for validating that functions return the expected values.

```
// if function doesn't return 4, it throws an exception
4 assert=? double 2

// value must be true or false
assertTrue? someFunction
assertFalse? someOtherFunction

// if assertFail is reached, throw an exception
if somethingWeExpectToFail
	assertFail ." Uh-oh, that function did the wrong thing
```
