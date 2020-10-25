

using SWNetwork.Core;

namespace SWNetwork.FrameSync
{
    public abstract class SWFrameSyncSystem
    {
        public abstract int GenerateSystemDataHash();
        public abstract int Export(SWBytes buffer);
        public abstract void Import(SWBytes buffer);

        public abstract bool Compare(SWFrameSyncSystem other);

        public virtual void Start()
        {

        }

        public virtual void WillUpdate()
        {

        }

        public virtual void Update(FrameSyncGame game, FrameSyncInput input, FrameSyncUpdateType updateType)
        {

        }

        public virtual void Stop()
        {
           
        }

    }
}
