using Newtonsoft.Json;
using OurIdolBot.Containers.PicturesContainers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OurIdolBot.Services.PicturesServices
{
    public class NekosLifeImageService
    {
        public NekosFileImage GetImage(string endpoint)
        {
            var client = new WebClient();
            var url = client.DownloadString(endpoint);
            return JsonConvert.DeserializeObject<NekosFileImage>(url);
        }
    }
}
