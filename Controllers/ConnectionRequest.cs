using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Controllers
{
   
    class ConnectionRequest
    {
        public String Request;
        public IPAddress SubnetworkIn;
        public IPAddress SubnetworkOut;

        public ConnectionRequest(IPAddress subnetworkIn, IPAddress subnetworkOut)
        {
            SubnetworkIn = subnetworkIn;
            SubnetworkOut = subnetworkOut;
        }

        public byte[] convertReqToByte()
        {
            List<byte> new_list = new List<byte>();
            new_list.AddRange(Encoding.ASCII.GetBytes("CCRC2"));
            new_list.AddRange(SubnetworkIn.GetAddressBytes());
            new_list.AddRange(SubnetworkOut.GetAddressBytes());
            return new_list.ToArray();
        }
    }
}
