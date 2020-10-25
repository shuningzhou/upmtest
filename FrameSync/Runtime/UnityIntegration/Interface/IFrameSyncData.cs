using SWNetwork.Core;
using System.Collections.Generic;

namespace SWNetwork.FrameSync
{
    interface IFrameSyncData
    {
        void FrameSyncDataInitialize(FrameSyncGame game);
        void Export(SWBytes buffer);
        void Import(SWBytes buffer);

        //Debug
        void ExportDebugInfo(Dictionary<string, string> debugDictionary);
    }
}
