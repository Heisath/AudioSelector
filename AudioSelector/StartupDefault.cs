using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AudioSelector
{
    public class StartupDefault
    {
        [XmlAttribute]
        public string DefaultOption { get; set; }
        [XmlElement("DeviceVolume")]
        public List<DeviceVolume> DeviceVolumes { get; set; }
      
    }
}

