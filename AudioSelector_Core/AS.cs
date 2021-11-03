using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AudioSelector_Core
{
    [XmlRoot("AudioSelector")]
    public class AS
    {
        [XmlElement("Startup")]
        public StartupDefault Startup { get; set; }

        [XmlElement("Option")]
        public List<Option> Options { get; set; }
        
    }
}
