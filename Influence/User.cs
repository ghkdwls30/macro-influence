using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Influence
{
    class User
    {
        public string nickNm { get; set; }
        public List<Hash> hashList { get; set; }
    }

    class Hash
    {
        public string nickNm { get; set; }
        public string hashNm { get; set; }
        public long totCnt { get; set; }
        public long workCnt { get; set; }
        public string workYmd { get; set; }
    }

    class IpHistory
    {
        public string ymdt { get; set; }
        public string ip { get; set; }
    }
}
