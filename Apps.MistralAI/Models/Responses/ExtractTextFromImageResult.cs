using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Files;

namespace Apps.MistralAI.Models.Responses
{
    public class ExtractTextFromImageResult
    {
        [Display("Json data file")]
        public FileReference JsonDataFile { get; set; }

        [Display("Content file")]
        public FileReference ContentFile { get; set; }
    }
}
