using System;
namespace Parallel
{
    public class PCollision3D
    {
        public IParallelRigidbody3D otherRigidbody { get; private set; }

        public Fix64Vec3 relativeVelocity
        {
            get
            {
                return _contact.RelativeVelocity;
            }
        }

        PContact3D _contact;
        UInt16 _otherBodyID;

        internal void SetContact(PContact3D contact, IParallelRigidbody3D rigidBody)
        {
            _contact = contact;
            otherRigidbody = rigidBody;
        }

        public int GetContactPoints(ref PContactPoints2D contactPoints2D)
        {
            return 0;
            //ParallelPhysics.GetContactDetail(_contact.IntPointer, ref contactPoints2D);
            //return contactPoints2D.contactPointCount;
        }
    }
}
