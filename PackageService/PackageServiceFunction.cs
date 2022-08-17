using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Polly;
using DroneDelivery.Common.Services;
using Polly.CircuitBreaker;

namespace PackageService
{
    public static class PackageServiceFunction
    {
        private static AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;
        static void Program()
        {
            _circuitBreakerPolicy = Policy
                    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                    .CircuitBreakerAsync(1, TimeSpan.FromSeconds(10), OnBreak, OnReset, OnHalfOpen);
        }

        [FunctionName("PackageServiceFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "packages/{id}")] HttpRequest req,
            string id, ILogger log)
        {
            Utility.DoWork(100);
            var apiClient = new HttpClient();
            int i = 0;
            while (true)
            {
                i++;
                Console.WriteLine($"{i}. Start calling to Web API");
                Console.WriteLine("\n");
                Console.WriteLine("-------------------------------------------------------------------------------------------");
                // Start calling to WebAPI
                var apiResponse = new HttpResponseMessage();
                try
                {
                    apiResponse = await _circuitBreakerPolicy.ExecuteAsync(
                        () => apiClient.PostAsync("http://example.com", null)
                        );
                    var json = await apiResponse.Content.ReadAsStringAsync();
                    // End calling to WebAPI

                    Console.WriteLine($"Http Status Code: {apiResponse.StatusCode}");
                    Console.WriteLine("\n");
                    Console.WriteLine($"Response: {json}");
                    Console.WriteLine("\n");
                    Console.WriteLine($"{i}. End calling to Web API");
                    Console.WriteLine("\n");
                    Console.WriteLine("-------------------------------------------------------------------------------------------");
                    Console.WriteLine("Type any key and press Enter to make new calling to Web API");
                    Console.WriteLine("-------------------------------------------------------------------------------------------");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }

            }
        }

        private static void OnHalfOpen()
        {
            Console.WriteLine("Connection half open - Circuit Breaker State is HALF-OPEN");
        }

        private static void OnReset(Context context)
        {
            Console.WriteLine("Connection reset - Circuit Breaker State is CLOSED");
        }

        private static void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeSpan, Context context)
        {
            Console.WriteLine("Connection is Closed - Circuit Breaker State is OPEN");
        }
    }
}
