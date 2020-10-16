using System;
namespace Parallel
{
    public class PCollision2D
    {
        public int contactCount
        {
            get
            {
                return _contact.ContactCount;
            }
        }

        public IParallelRigidbody2D otherRigidbody { get; private set; }

        public Fix64Vec2 relativeVelocity
        {
            get
            {
                return _contact.RelativeVelocity;
            }
        }

        PContact2D _contact;
        UInt16 _otherBodyID;

        internal void SetContact(PContact2D contact, IParallelRigidbody2D rigidBody)
        {
            _contact = contact;
            otherRigidbody = rigidBody;
        }

        public int GetContactPoints(ref PContactPoints2D contactPoints2D)
        {
            Parallel2D.GetContactDetail(_contact.IntPointer, ref contactPoints2D);
            return contactPoints2D.contactPointCount;
        }
    }
}
