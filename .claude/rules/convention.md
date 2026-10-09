---
paths: "**/*.cs"
---

# Convention

- Use body constructor instead of primary constructor.
- Use `private readonly` fields assigned in the constructor, like `UserService`
- When you come across a class that still has a primary constructor, suggest converting it to a body constructor; convert it only once the user agrees.
- Positional records (`record CompanySummaryDto(Guid Id, …)`) are not concerned: they stay positional.
