using System;
namespace Parallel
{
    public class PContact3D
    {
        internal ContactState state;

        internal UInt32 ContactID { get; private set; }
        internal UInt16 Body1ID { get; private set; }
        internal UInt16 Body2ID { get; private set; }
        internal int ContactCount { get; private set; }
        internal Fix64Vec3 RelativeVelocity { get; private set; }
        internal IntPtr IntPointer { get; private set; }
        internal bool IsTrigger { get; private set; }

        public void Update(
            IntPtr nativeHandle,
            int contactCount,
            Fix64Vec3 relativeVelocity,
            bool isTrigger
            )
        {
            IntPointer = nativeHandle;
            ContactCount = contactCount;
            RelativeVelocity = relativeVelocity;
            IsTrigger = isTrigger;
        }

        public PContact3D(UInt32 contactID)
        {
            ContactID = contactID;
            Body1ID = (UInt16)(contactID % 100000);
            Body2ID = (UInt16)(contactID / 100000);
        }
    }
}
