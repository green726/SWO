# SWO

<aside>
❗ This is a work in progress, and not all pages are complete. Please use the Notion (First item in the links section at the bottom) when possible

</aside>

# About:

<aside>
ℹ️ [SWO](https://github.com/green726/SWO) is a general purpose low-level esoteric compiled programming language. The language is designed to be as customizable / configurable as possible through the use of a simple Toml file.

</aside>

### Philosophies:

- Be (easily) customizable without requiring users to write code
- Maintain decent speed/performance in the compiler
- Maintain speed in the language
- Have a good FFI (Foreign Function Interface) / Interoperability with other LLVM languages (C, C++, Rust, etc)

### Why?

Why does SWO exist? I created SWO as a fun resume-building project and to learn about compilers. 

### How?

SWO is written in C# and uses a custom build parser. SWO uses the LLVMSharp C# LLVM bindings to turn the SWO code into LLVM IR. LLVM is used by various major languages including (but not limited to) C, C++, Rust, Haskell, Julia, Swift. 

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
    print(fibRec(4))
}
```

# Pages:

[Documentation](Docs/Documentation%202fc9117d645f475fba9140278701c2a2.md)

# Roadmap:

# Links:

Notion: [https://ivy-turquoise-4f8.notion.site/SWO-c193b980d415499c9103a9716067a5ba](https://ivy-turquoise-4f8.notion.site/SWO-c193b980d415499c9103a9716067a5ba)

Internal Documentation: [https://www.notion.so/SWO-Internal-cb00b30071df4b7e80e22baf3b43df62](https://www.notion.so/SWO-Internal-cb00b30071df4b7e80e22baf3b43df62)

[https://github.com/green726/SWO](https://github.com/green726/SWO)

---
