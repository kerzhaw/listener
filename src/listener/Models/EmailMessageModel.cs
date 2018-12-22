using System.Collections.Generic;

namespace listener.Models
{
    public class EmailMessageModel
    {
        public string TemplateName { get; set; }
        public string TemplateData { get; set; }
        public List<string> To { get; set; }
        public string From { get; set; }
    }
}