# Declaration

<aside>
ℹ️ This page contains documentation for declaring variables with SWO (default configuration)

</aside>

# Examples:

```c
//constants
const int foo = 1
//variables (mutable)
var int bar = 2
//strings
const string hello = "hello"
//arrays
const int[] fooArr = {1, 2, 3}
//array with no default value
const int[3] fooArr
//declare a pointer
const int* foo
```

---

# Explanation:

SWO variable declarations are very straightforward and similar to all other C based languages.

You first (optionally) mark a variable declaration with `var`(mutable) or `const`(immutable). Next, you follow with the variable’s type. After the type comes the name, and then finally the default value. 

The one unique thing that SWO does is mandate that pointers are defined with their type (`int*`) which must be the type pointed to followed by an asterisk (or your configured pointer symbol)