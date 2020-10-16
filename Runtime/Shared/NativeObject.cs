using System;
namespace Parallel
{
    public class NativeObject
    {
        public IntPtr m_NativeObject = IntPtr.Zero;

        public NativeObject(IntPtr nativeHandle)
        {
            m_NativeObject = nativeHandle;
        }

        internal IntPtr IntPointer
        {
            get
            {
                return m_NativeObject;
            }
        }
    }
}
