using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseR.Common.Serialization
{
    public interface IPartialSerializable
    {
        string SerializePart();
        void DeserializePart(string value);
    }
}
