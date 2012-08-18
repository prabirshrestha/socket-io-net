using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IdGeneratorFunc = System.Func<
    System.Collections.Generic.IDictionary<string, object>, // environment
    System.Collections.Generic.IDictionary<string, string[]>, // headers
    string>; // generated id

namespace SocketIoDotNet
{
    public class SocketIoConfig
    {
        private IdGeneratorFunc _generateId = DefaultIdGenerator;

        public IdGeneratorFunc GenerateId
        {
            get { return _generateId ?? (_generateId = DefaultIdGenerator); }
            set { _generateId = value; }
        }

        private static string DefaultIdGenerator(IDictionary<string, object> environment, IDictionary<string, string[]> headers)
        {
            return "autogenid1";
        }
    }
}
