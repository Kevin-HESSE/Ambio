---
paths: "**/*.cs"
---

# Convention

## Constructor
- Use body constructor instead of primary constructor.
- Use `private readonly` fields assigned in the constructor, like `UserService`
- When you come across a class that still has a primary constructor, suggest converting it to a body constructor; convert it only once the user agrees.
- Positional records (`record CompanySummaryDto(Guid Id, …)`) are not concerned: they stay positional.

## Named arguments
- A positional record built in a mapping takes named arguments, one per line, so each DTO member shows its source :

  ```csharp
  offers.Select(o => new JobOfferSummaryDto(
      Id: o.Id,
      Title: o.Title,
      CompanyName: o.Company.Name,
      RegisterAt: o.CreatedAt));
  ```
