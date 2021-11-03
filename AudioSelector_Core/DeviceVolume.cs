using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AudioSelector_Core
{
    public class DeviceVolume
    {
        [XmlAttribute]
        public string DeviceName { get; set; }
        [XmlAttribute]
        public int Volume { get; set; }
    }
}

