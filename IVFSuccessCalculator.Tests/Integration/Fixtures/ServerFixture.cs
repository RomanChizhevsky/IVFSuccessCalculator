using System.Net.Http.Json;
using System.Net.Sockets;
using IVFSuccessCalculator.Models;

namespace IVFSuccessCalculator.Tests.Integration.Fixtures
{
    public class ServerFixture : IDisposable
    {
        // This should mirror the configuration stipulated for the web servers' launch settings.JSON
        private const int SERVER_LISTENING_ON_PORT = 7174;

        private readonly HttpClient _httpClient = new();
        private readonly string _url = $"https://localhost:{SERVER_LISTENING_ON_PORT}/success-rate";

        private readonly bool _serverIsUp;

        public ServerFixture()
        {
            _serverIsUp = CheckServerIsUp();
        }

        public bool CheckServerIsUp()
        {
            using var tcpClient = new TcpClient();

            try
            {
                tcpClient.Connect("localhost", SERVER_LISTENING_ON_PORT);
            }
            catch (SocketException e) when (e.SocketErrorCode == SocketError.ConnectionRefused)
            {
                return false;
            }

            return true;
        }

        public Task<HttpResponseMessage> CalculateSuccessRate(SuccessRateCalculationRequest request)
        {
            if (!_serverIsUp)
                throw new InvalidOperationException("Cannot connect to calculator web server. Please ensure that it is spun up prior to running integration tests.");

            return _httpClient.PostAsJsonAsync(_url, request);
        }

        public void Dispose()
        {
        }
    }
}
