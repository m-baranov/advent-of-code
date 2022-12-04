using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AdventOfCode2022
{
    public interface IInput
    {
        Task<TextReader> Load();
    }

    public class LiteralInput : IInput
    {
        private readonly IReadOnlyList<string> lines;

        public LiteralInput(IReadOnlyList<string> lines)
        {
            this.lines = lines;
        }

        public Task<TextReader> Load()
        {
            var input = string.Join("\n", this.lines);
            var reader = new StringReader(input);
            return Task.FromResult(reader as TextReader);
        }
    }

    public class HttpInput : IInput
    {
        private readonly HttpClient client;
        private readonly string url;
        private readonly string session;

        public HttpInput(HttpClient client, string url, string session)
        {
            this.client = client;
            this.url = url;
            this.session = session;
        }

        public async Task<TextReader> Load()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Cookie", $"session={this.session};");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            return new StreamReader(stream);
        }
    }

    public class CachingInput : IInput
    {
        private readonly string key;
        private readonly IInput innerInput;

        public CachingInput(string key, IInput innerInput)
        {
            this.key = key;
            this.innerInput = innerInput;
        }

        public async Task<TextReader> Load()
        {
            var fileName = key + ".txt";

            if (!File.Exists(fileName))
            {
                var reader = await innerInput.Load();
                File.WriteAllText(fileName, reader.ReadToEnd());
            }

            return File.OpenText(fileName);
        }
    }

    public static class Input
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        public static string HttpSession = "unknown-session";

        public static IInput Literal(params string[] lines) => new LiteralInput(lines);

        public static IInput Http(string url) 
        {
            var cacheKey = url.Replace(':', '_').Replace('.', '_').Replace('/', '_');
            return new CachingInput(cacheKey, new HttpInput(HttpClient, url, HttpSession));
        }

        public static IEnumerable<string> Lines(this TextReader reader)
        {
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                yield return line;
            }
        }
    }

    public interface IProblem
    {
        void Run(TextReader input);
    }

    public static class Problem
    {
        public static async Task Run(this IProblem problem, IInput input)
        {
            using var inputReader = await input.Load();
            problem.Run(inputReader);
        }
    }
}
