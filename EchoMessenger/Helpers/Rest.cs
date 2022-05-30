using RestSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EchoMessenger.Helpers
{
    public class Rest
    {
        private RestClient client;
        private RestClientOptions options;
        private CancellationToken cancellationToken;

        public Rest(String host)
        {
            options = new RestClientOptions(host)
            {
                ThrowOnAnyError = false,
                Timeout = 5000
            };
            client = new RestClient(options);
            cancellationToken = new CancellationToken();
        }

        public async Task<RestResponse?> Get(String target, Object? json = null)
        {
            var request = new RestRequest(target, Method.Get)
                .AddHeader("Content-Type", "application/json");
                
            if (json != null)
            {
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(json);
            }

            var jwt = RegistryManager.GetCurrentJwt();

            if (!String.IsNullOrEmpty(jwt))
                request.AddHeader("authorization", jwt);

            try
            {
                var response = await client.ExecuteAsync(request, cancellationToken);
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<RestResponse?> Post(String target, Object? json = null)
        {
            var request = new RestRequest(target, Method.Post)
                .AddHeader("Content-Type", "application/json");

            if (json != null)
            {
                request.RequestFormat = DataFormat.Json;
                request.AddJsonBody(json);
            }

            var jwt = RegistryManager.GetCurrentJwt();

            if (!String.IsNullOrEmpty(jwt))
                request.AddHeader("authorization", jwt);

            try
            {
                var response = await client.ExecuteAsync(request, cancellationToken);
                return response;
            }
            catch
            {
                return null;
            }
        }
    }
}
