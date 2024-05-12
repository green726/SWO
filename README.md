# SWO

# About:

<aside>
ℹ️ SWO is a general purpose low-level compiled programming language. The language is designed to be as customizable / configurable as possible through the use of a simple Toml file.

</aside>

### Philosophy:

- Be (easily) customizable without requiring users to write code
- Maintain decent speed/performance in the compiler
- Maintain speed in the language
- Have a good FFI (Foreign Function Interface) / Interoperability with C (and maybe C++)

### Why?

Why does SWO exist? I created SWO as a fun project to learn about compilers. 

### How?

SWO is written in C# and uses a custom built parser. SWO uses the LLVMSharp C# LLVM bindings to translate the SWO code into LLVM Intermediary Representation (IR). This is then compiled (by LLVM) to native executables and/or binaries on a vareity of platforms. LLVM is used by various major languages including (but not limited to) C, C++, Rust, Haskell, Julia, Swift. 

### A quick peek:

```c
int @fibRec(int n) {
    if (n == 1) {
        return n
    }
    if (n == 0) {
        return n
    }
    return fibRec(n - 1) + fibRec(n - 2)
}

@main() {
    printf("%d", fibRec(4))
}
```
