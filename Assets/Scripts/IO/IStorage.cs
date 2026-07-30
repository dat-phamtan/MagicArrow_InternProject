using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.IO
{
    public interface IStorage
    {
        T Load<T>(string fileName, T defaultData = default);
        void Save<T>(string fileName, T data);
        void Append<T>(string fileName, T data);
    }
}
