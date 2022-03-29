using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WordleHelper.Core;

namespace WordleHelper.Tests
{
    [TestClass]
    public class WordFindingTests
    {
        public WordFindingTests()
        {
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

        [TestMethod]
        public async Task BlankSkeletonAsync()
        {
            var input = new List<string>() { "spark", "hello" };
            var logic = CreateWordleHelperLogic(input);
            await logic.GetWords("https://google.com");
            var output = logic.GetSolutions(new WordleHelperInput());
            CollectionAssert.AreEquivalent(input, output.Solutions.ToList());
        }

        [TestMethod]
        public async Task FilledSkeletonAsync()
        {
            var input = new List<string>() { "spark", "hello" };
            var logic = CreateWordleHelperLogic(input);
            await logic.GetWords("https://google.com");
            var model = new WordleHelperInput()
            {
                Skeleton = "s----"
            };
            var output = logic.GetSolutions(model);
            CollectionAssert.AreEquivalent(new List<string>() { "spark" }, output.Solutions.ToList());
        }

        [TestMethod]
        public async Task BlockedAsync()
        {
            var input = new List<string>() { "spark", "hello" };
            var logic = CreateWordleHelperLogic(input);
            await logic.GetWords("https://google.com");
            var model = new WordleHelperInput()
            {
                BlockedLetters = "h"
            };
            var output = logic.GetSolutions(model);
            CollectionAssert.AreEquivalent(new List<string>() { "spark" }, output.Solutions.ToList());
        }

        [TestMethod]
        public async Task WrongSkeletonAsync()
        {
            var input = new List<string>() { "spark", "naked" };
            var logic = CreateWordleHelperLogic(input);
            await logic.GetWords("https://google.com");
            var model = new WordleHelperInput()
            {
                WrongPositionSkeleton = "--k--"
            };
            var output = logic.GetSolutions(model);
            CollectionAssert.AreEquivalent(new List<string>() { "spark" }, output.Solutions.ToList());
        }

        [TestMethod]
        public async Task KnownLettersAsync()
        {
            var input = new List<string>() { "spark", "hello" };
            var logic = CreateWordleHelperLogic(input);
            await logic.GetWords("https://google.com");
            var model = new WordleHelperInput()
            {
                KnownLetters = "k"
            };
            var output = logic.GetSolutions(model);
            CollectionAssert.AreEquivalent(new List<string>() { "spark" }, output.Solutions.ToList());
        }

        [TestMethod]
        public async Task FullAsync()
        {
            var input = new List<string>() { "spark", "naked" };
            var logic = CreateWordleHelperLogic(input);
            await logic.GetWords("https://google.com");
            var model = new WordleHelperInput()
            {
                Skeleton = "s----",
                WrongPositionSkeleton = "--k--",
                KnownLetters = "k",
                BlockedLetters = "e"
            };
            var output = logic.GetSolutions(model);
            CollectionAssert.AreEquivalent(new List<string>() { "spark" }, output.Solutions.ToList());
        }
    }
}