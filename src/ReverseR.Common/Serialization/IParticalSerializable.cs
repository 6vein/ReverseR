using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.Serialization
{
    public interface IParticalSerializable
    {
        string SerializePart();
        void DeserializePart(string value);
    }
}
