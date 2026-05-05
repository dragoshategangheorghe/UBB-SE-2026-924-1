# Coding Style and Conventions Document
This document defines how everybody should write their code in the team.

## Naming Conventions
1. Use PascalCase for class names and method names.
2. Use `camelCase` for local variables and method parameters.
3. Interface names must start with the letter `I` followed by `PascalCase` (e.g., `IRepository`).
4. Private class fields must start with an underscore `_` followed by `camelCase` (e.g., `_dbContext`).
5. Use `PascalCase` for public properties.
6. Constants should be written in `PascalCase` (do not use UPPER_SNAKE_CASE).

## Formatting and Structure
7. Use 4 spaces for indentation; do not use tabs.
8. Braces `{` and `}` must be placed on a new line (Allman style).
9. Each file must contain a single class or interface.
10. The file name must exactly match the name of the class or interface it contains.
11. All `using` directives must be placed at the top of the file.
12. A line of code should not exceed 120 characters.

## Syntax and Best Practices
13. Use `var` only when the type of the variable is obvious from the right side of the assignment.
14. Use string interpolation (`$"{variable}"`) instead of string concatenation with `+`.
15. Asynchronous methods must end with the `Async` suffix.
16. Always add access modifiers explicitly (`public`, `private`, etc.).
17. Do not catch generic exceptions (`catch (Exception)`) unless you are logging them globally.
18. Use only `throw;` to re-throw an exception so you do not lose the original stack trace.
19. Comments should explain "why" the code is written a certain way, not "what" it does (the code should be self-explanatory).
20. Your solution must not contain any business logic in its UI or its data access layer.
