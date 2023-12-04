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

        // Filter for found letters
        var foundLettersRegexTemplate = foundLetters.Aggregate("", (regexString, next) => regexString += $@"(?=\w*{next}\w*)");

        // Get Correct Letters
        // Filter out blocked
        // Filter out wrong positions
        var index = 0;
        var blockedLettersRegexTemplate = model.WrongPositionSkeleton.Aggregate(@"\b", (regexString, next) =>
        {
            var item = "";
            if (model.Skeleton[index] != '-')
            {
                item = model.Skeleton[index].ToString();
            }
            else if (next == '-')
            {
                item = model.BlockedLetters == string.Empty ? "." : $"[^{model.BlockedLetters}]";
            }
            else
            {
                item = model.BlockedLetters == string.Empty ? $"[^{next}]" : $"[^{model.BlockedLetters}{next}]";
            }
            index++;
            return regexString += item;
        }) + @"\b";

        var regexTemplate = foundLettersRegexTemplate + blockedLettersRegexTemplate;
        Console.WriteLine(regexTemplate);
        var potentialMatches = _words.Where(word => Regex.IsMatch(word, regexTemplate, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500)));       

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