# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project: Docs and Plannings

Docs and Plannings is a high-performance convenient ASP.net web application that allows to create/store documentation and create/track tasks/issues.

> **Scope**
> Applies to every C# project in this repository (libraries, applications, tests).
> The agent MUST follow all mandatory rules (🔒). Guidelines (📝) describe the desired style; deviate only with strong justification.

---

## 1. Directory layout (example)

```
.
├── docs/           # All documentation about features
├── plans/          # Current feature development plans
├── screenshots/    # All screenshots should be placed to this folder
├── source/         # Code base with projects
├── tests/          # Unit-test assemblies mirror src/ structure
├── CLAUDE.md       # ← this file
└── README.md
```

*The exact folder names may differ, but the logical split **src / tests** is mandatory.* 🔒  

---

## 2. Environment, build & test commands (CI parity)

```bash
# 0. Verify .NET SDK presence (install if missing)
if ! command -v dotnet >/dev/null 2>&1; then
    echo ".NET SDK not found — installing ..."
    # *Sample scripted install for Ubuntu; adapt for your OS*
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel LTS
    export DOTNET_ROOT="$HOME/.dotnet"
    export PATH="$PATH:$DOTNET_ROOT"
fi
dotnet --info          # must print SDK details

# 1. Restore & build (treat warnings as errors)
dotnet restore
dotnet build --configuration Release -warnaserror

# 2. Optional code-style fix
dotnet format
```

The agent must **never** commit code unless all commands above succeed locally 🔒  
---

## 3. Mandatory rules

1. **Unit tests first** — every non-trivial change adds or updates tests. 🔒  
2. **Assert critical logic & parameters** using `Debug.Assert` or unit-test assertions. 🔒  
3. **Code comments only in English.** 🔒  
4. Enable  
   * `nullable` reference types,  
   * `implicit usings`,  
   * `TreatWarningsAsErrors` (default warning level). 🔒  
5. No **global usings**, **singletons**, **local functions**, **dynamic**, **file-scope `internal`/`private protected` modifiers**, **LINQ query syntax**. 🔒  
6. Static code must be **thread-safe** and side-effect-free. 🔒  
7. All file IO wrapped in `try / catch` with the narrowest possible scope. 🔒  
8. Filenames = top-level type; one top-level type per file. 🔒  
9. Usings → Namespace → Members. 🔒  
10. Class / struct member order  
    1. Constants  
    2. Events  
    3. Fields  
    4. Properties & Indexers  
    5. Constructors  
    6. Methods 🔒  
11. Interface member order: Properties, then Methods. 🔒  
12. **Verify external source links** — when user supplies links to code from other projects, ensure the integrated code exactly matches the referenced source or update with justification. 🔒  
13. **P/Invoke & external DLLs** — every interop call **must** be covered by unit tests that validate correct marshaling and error handling. 🔒  
14. **Tests must genuinely reflect reality** — a test must fail for any defect (e.g., logic error, missing dependency); absence of a required DLL or resource **must not** yield a passing test. 🔒  
15. **Unsafe code must be only in dedicated profects, wrapped with our own binding classes and only those classes must be used** 🔒  
16. **Always update roadmap file** 🔒  
17. **For each feature using git branches** 🔒  
18. **All work should be commited to the branch** 🔒  
19. **Screenshots should be placed into screenshots folder in project** 🔒  
20. **Never commit screenshots to repo. It should only be stored locally** 🔒  
21. **At the end of any task report which skills you used and where** 🔒  
22. **Always ensure that all changes are not breaking any of the request sections and 100% fullfills to its requirements 🔒
---

## 4. Pull-request checklist

- [ ] All unit tests green (`dotnet test`).
- [ ] All unit tests green ('dotnet test --configuration Release')
- [ ] No access violation issues during tests
- [ ] No compiler warnings (`dotnet build -warnaserror`).
- [ ] Style passes (`dotnet format`).
- [ ] Added/updated tests, including for all P/Invoke and external DLL usages.
- [ ] Verified imported external sources match the original links.
- [ ] **Member ordering verified** (Rules 10 & 11) - .editorconfig cannot enforce this automatically.

---

## 5. Restricted areas

- Do **not** commit secrets or credentials. 🔒  
- Do **not** introduce additional project/package managers (npm, pip, etc.) without discussion. 🔒  

---

## 6. Contact

For questions that cannot be resolved by this guideline, tag a maintainer in the PR description and provide a concise rationale.
