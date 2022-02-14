using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace WordleHelper;

public interface IWordleHelperLogic
{
    Task GetWords();
    IEnumerable<string> GetSolutions(WordleHelperInput model);
}

public class WordleHelperLogic : IWordleHelperLogic
{
    private readonly HttpClient _http;
    private string _words = "";

    public WordleHelperLogic(HttpClient http)
    {
        _http = http;
    }

    public async Task GetWords()
    {
        var response = await _http.GetFromJsonAsync<DictionaryData>("dictionaries/wordle.json");
        if (response is not null)
        {
            var rnd = new Random();
            var randomized = response.Words.OrderBy(item => rnd.Next());
            _words = string.Join(",", randomized);
        }
    }

    public IEnumerable<string> GetSolutions(WordleHelperInput model)
    {
        var blockedRegex = model.BlockedLetters.Length > 0 ? $"[^{model.BlockedLetters}]" : ".";
        var regexBuilder = @"\b";
        foreach (var letter in model.Skeleton)
        {
            if (letter == '_')
                regexBuilder += blockedRegex;
            else
                regexBuilder += (letter.ToString());
        }
        regexBuilder += @"\b";

        Regex regex = new Regex(regexBuilder, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var potentialMatches = regex.Matches(_words).Select(m => m.Value).ToList();
        var matches = potentialMatches;

        if (!string.IsNullOrEmpty(model.ConfirmedLetters))
        {
            matches = new List<string>();
            potentialMatches.ForEach(match =>
            {
                var passed = true;
                model.ConfirmedLetters.ToCharArray().ToList().ForEach(letter =>
                {
                    if (!match.Contains(letter)) passed = false;
                });
                if (passed) matches.Add(match);
            });
        }
        return matches.Take(100);
    }
}