﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using ResultTuple = System.Tuple< //Result
    System.Collections.Generic.IDictionary<string, object>, // Properties
    int, // Status
    System.Collections.Generic.IDictionary<string, string[]>, // Headers
    System.Func< // CopyTo
        System.IO.Stream, // Body
        System.Threading.Tasks.Task>>; // Done

namespace SocketIoDotNet.Transports
{
    public class SocketIoJsonpTransport : ISocketIoTransport
    {
        public string Name
        {
            get { return "jsonp-polling"; }
        }

        public async Task<ResultTuple> HandleRequest(string id, IDictionary<string, object> environment, IDictionary<string, string[]> headers, Stream body)
        {
            throw new NotImplementedException();
        }
    }
}