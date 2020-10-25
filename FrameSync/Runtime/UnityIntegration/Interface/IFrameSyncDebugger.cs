using SWNetwork.Core;

namespace SWNetwork.FrameSync
{
    public interface IFrameSyncDebugger
    {
        void Initialized(FrameSyncAgent agent);
        void WillStep(FrameSyncEngine engine, FrameSyncGame game);
        void DidStep(FrameSyncEngine engine, FrameSyncGame game);
    }
}
