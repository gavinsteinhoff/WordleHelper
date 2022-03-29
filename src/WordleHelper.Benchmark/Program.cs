using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using WordleHelper.Core;

namespace WordleHelper.Benchmarks;

public class WordleBenchmarkClass
{
    private readonly WordleHelperLogic _logic;
    private readonly WordleHelperInput _model;

    public WordleBenchmarkClass()
    {
        _logic = CreateWordleHelperLogic(BenchmarksInputWords.GetInputWords().ToList());
        _logic.GetWords("https://example.com").Wait();
        _model = new WordleHelperInput()
        {
            Skeleton = "s----",
            WrongPositionSkeleton = "--a--",
            KnownLetters = "p",
            BlockedLetters = "k"
        };
    }

    private static WordleHelperLogic CreateWordleHelperLogic(List<string> input)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        var response = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(input))
        };

        handlerMock
          .Protected()
          .Setup<Task<HttpResponseMessage>>(
             "SendAsync",
             ItExpr.IsAny<HttpRequestMessage>(),
             ItExpr.IsAny<CancellationToken>())
          .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);

        return new(httpClient);
    }

    [Benchmark]
    public void BenchmarkWordleHelper() => _logic.GetSolutions(_model);
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<WordleBenchmarkClass>();
        Console.WriteLine(summary);
    }
}