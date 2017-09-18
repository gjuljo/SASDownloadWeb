using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using Microsoft.AspNetCore.Http;
using SASGeneratorWeb;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace RazorPages
{
    public class IndexModel : PageModel
    {

        private readonly StorageConfiguration _config;
        private readonly SASGenerator _generator;

        public IndexModel(IOptions<StorageConfiguration> storageConfig )
        {
            _config = storageConfig.Value;
            _generator = new SASGenerator(_config.AccountName, _config.AccountKey, _config.ContainerName);
        }

        public string Message { get; private set; } = ".NET Core in C#";

        public string FileURI { get; private set; }
        public string FileName { get; private set; }


        public async Task OnGetAsync()
        { 
            string remoteip = HttpContext.Connection.RemoteIpAddress.ToString();

            Message += $" serving {remoteip}. Server time is { DateTime.Now }";

            FileName = _config.FileName; 
            FileURI  = await _generator.GenerateURI(remoteip, FileName);
        }
    }
}