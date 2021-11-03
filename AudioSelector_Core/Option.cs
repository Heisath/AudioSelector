using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AudioSelector_Core
{
    public class Option
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlElement]
        public string DefaultMultimediaOut { get; set; }
        [XmlElement]
        public string DefaultMultimediaIn { get; set; }
        [XmlElement]
        public string DefaultCommunicationsOut { get; set; }
        [XmlElement]
        public string DefaultCommunicationsIn { get; set; }
    }
}

