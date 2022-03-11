using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace WordleHelper.Core;

public interface IWordleHelperLogic
{
    Task GetWords(string wordList = "dictionaries/wordle.json");
    WordleSolution GetSolutions(WordleHelperInput model);
}

public class WordleHelperLogic : IWordleHelperLogic
{
    private readonly HttpClient _http;
    private IEnumerable<string> _words = new List<string>();

    public WordleHelperLogic(HttpClient http)
    {
        _http = http;
    }

    public async Task GetWords(string? wordList = "dictionaries/wordle.json")
    {
        var response = await _http.GetFromJsonAsync<List<string>>(wordList);
        if (response is not null)
        {
            var rnd = new Random();
            var randomized = response.OrderBy(item => rnd.Next());
            _words = randomized;
        }
    }

    public WordleSolution GetSolutions(WordleHelperInput model)
    {
        var foundLetters = string.IsNullOrEmpty(model.KnownLetters) ? string.Empty : model.KnownLetters;
        foundLetters += model.WrongPositionSkeleton.Replace("-", "");

        // Match correct skeleton
        var matchedRegexTemplate = model.Skeleton.Aggregate(@"\b", (regexString, next) => regexString + (next == '-' ? '.' : next)) + @"\b";
        var matchedRegex = new Regex(matchedRegexTemplate, RegexOptions.IgnoreCase);
        var potentialMatches = _words.Where(m => matchedRegex.IsMatch(m));

        // Filter to found letters
        var foundLettersRegexTemplate = foundLetters.Aggregate("", (regexString, next) => regexString += $"(?=.*{next}.*)") + @"\w*";
        var foundRegex = new Regex(foundLettersRegexTemplate, RegexOptions.IgnoreCase);
        potentialMatches = potentialMatches.Where(m => foundRegex.IsMatch(m));

        // Filter out blocked
        // Filter out wrong positions
        var blockedLettersRegexTemplate = model.WrongPositionSkeleton.Aggregate(@"\b", (regexString, next) =>
        {
            var item = "";
            if (next == '-')
            {
                item = model.BlockedLetters == string.Empty ? "." : $"[^{model.BlockedLetters}]";
            }
            else
            {
                item = model.BlockedLetters == string.Empty ? $"[^{next}]" : $"[^{model.BlockedLetters}{next}]";
            }
            return regexString += item;
        }) + @"\b";
        var blockedRegex = new Regex(blockedLettersRegexTemplate, RegexOptions.IgnoreCase);
        potentialMatches = potentialMatches.Where(m => blockedRegex.IsMatch(m));

        var solutionText = potentialMatches.Take(30).Aggregate((current, i) => current + $"<p>{i}</p>");
        var count = potentialMatches.Take(30).Count();

        return new WordleSolution()
        {
            SolutionText = solutionText,
            Solutions = potentialMatches.Take(30),
            Count = count
        };
    }
}