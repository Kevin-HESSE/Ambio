using System.Text.RegularExpressions;

private var pattern = @"^(?=.{1,90}$)(?:build|feat|ci|chore|docs|fix|perf|refactor|revert|style|test)(?:\(.+\))*!?(?::)\s.{4,}(?<![\.\s])$";
// git revert of a single commit: Revert "<valid subject>"; a reverted merge (whole branch) is refused
private var revertPattern = @"^Revert ""(.+)""$";
private var lines = File.ReadAllLines(Args[0]);
private var msg = lines.Length > 0 ? lines[0] : string.Empty;

private bool IsValidSubject(string subject)
{
   if (Regex.IsMatch(subject, pattern))
      return true;
   var revert = Regex.Match(subject, revertPattern);
   return revert.Success && IsValidSubject(revert.Groups[1].Value);
}

// An optional body must be separated from the subject by a blank line
private bool HasBlankLineAfterSubject()
{
   return lines.Length < 2 || string.IsNullOrWhiteSpace(lines[1]) || lines[1].StartsWith('#');
}

if (IsValidSubject(msg) && HasBlankLineAfterSubject())
   return 0;

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine("Invalid commit message");
Console.ResetColor();
Console.WriteLine("e.g: 'feat(scope): subject' or 'fix: subject'");
Console.WriteLine("An optional body must follow a blank line; reverting a merge is only done through a pull request");
Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine("more info: https://www.conventionalcommits.org/en/v1.0.0/");

return 1;
