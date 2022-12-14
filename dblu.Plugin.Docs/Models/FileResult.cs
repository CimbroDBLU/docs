using System;
using System.Collections.Generic;

namespace dblu.Portale.Plugin.Docs.Models
{
    public class FileRisultato
    {
        public List<string> FileNames { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime UpdatedTimestamp { get; set; }
        public List<string> ContentTypes { get; set; }
    }
}