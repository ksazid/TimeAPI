using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TimeAPI.API.Serialization
{
    public interface ISerializer<T>
    {
        T Serialize(object obj);

        R Deserialize<R>(T stream);
    }
}
