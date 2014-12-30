using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Jack.Core.IO;

namespace Jack.Data
{
    public interface IDataStore
    {
        VersionManifest GetManifest(string Filename, long versionNumber);

        void PutManifest(Guid fileID, string filename, VersionManifest manifest);
    }
}
