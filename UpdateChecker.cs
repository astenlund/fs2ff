using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace fs2ff
{
    public static class UpdateChecker
    {
        public static async Task<UpdateInformation?> Check()
        {
            try
            {
                using var handler = new HttpClientHandler { AllowAutoRedirect = false };
                using var client = new HttpClient(handler);

                var response = await client.GetAsync(new Uri("https://github.com/jeffdamp-wave/fs2ff/releases/latest")).ConfigureAwait(false);
                var location = response.Headers.Location;
                var versionStr = location?.Segments[^1];

                return (Version.TryParse(versionStr?.TrimStart('v'), out var version) && version > App.AssemblyVersion)
                    ? new UpdateInformation(version, location!)
                    : null;
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync("Exception caught: " + e);
                return default;
            }
        }
    }
}
