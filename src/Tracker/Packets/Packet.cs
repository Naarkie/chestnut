using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tracker.Packets
{
    public class Packet
    {
        public List<object> Components { get; set; }
        public void FullBacking()
        {
            List<byte[]> bytes = new List<byte[]>();
            foreach(object component in Components)
            {
                //bytes.Add(BitConverter.GetBytes(component));
            }

            //return result;
        }
    }
}
