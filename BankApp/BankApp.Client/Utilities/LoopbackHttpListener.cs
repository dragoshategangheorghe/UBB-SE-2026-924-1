using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BankApp.Client.Utilities
{
    public class LoopbackHttpListener : IDisposable
    {
        private const int FiveMinutes = 60 * 5;
        private const int DefaultTimeout = FiveMinutes;

        private readonly HttpListener _listener;
        private readonly string _url;

        public LoopbackHttpListener(int port, string path = null)
        {
            path = path ?? string.Empty;
            if (path.StartsWith("/"))
            {
                path = path.Substring(1);
            }

            _url = $"http://127.0.0.1:{port}/{path}";

            _listener = new HttpListener();
            _listener.Prefixes.Add(_url);
            if (!_url.EndsWith("/"))
            {
                _listener.Prefixes.Add(_url + "/");
            }

            _listener.Start();
        }

        public async Task<string> WaitForCallbackAsync(int timeoutInSeconds = DefaultTimeout)
        {
            int timeoutInMilliseconds = 1000 * timeoutInSeconds;

            TaskCompletionSource<string> source = new TaskCompletionSource<string>();

            _listener.BeginGetContext(async result =>
            {
                try
                {
                    var context = _listener.EndGetContext(result);
                    var request = context.Request;
                    var response = context.Response;

                    string responseString = "<html><head><style>body{font-family:sans-serif;display:flex;justify-content:center;align-items:center;height:100vh;margin:0;background:#f0f2f5;} .card{background:white;padding:2rem;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,0.1);text-align:center;}</style></head><body><div class='card'><h2>Authentication Complete!</h2><p>You can now safely close this browser tab and return to the Bank App.</p></div></body></html>";
                    var buffer = Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;

                    var responseOutput = response.OutputStream;
                    await responseOutput.WriteAsync(buffer, 0, buffer.Length);
                    responseOutput.Close();

                    source.SetResult(request.Url!.ToString());
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            }, _listener);

            await Task.WhenAny(source.Task, Task.Delay(timeoutInMilliseconds));

            if (!source.Task.IsCompleted)
            {
                throw new TaskCanceledException("Browser authentication timed out.");
            }

            return await source.Task;
        }

        public void Dispose()
        {
            _listener?.Stop();
        }
    }
}
