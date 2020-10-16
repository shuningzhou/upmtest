using System;
using Parallel;

namespace Parallel
{
    public enum ParallelForceMode
    {
        Force = 0,
        Acceleration = 1,
        Impulse = 2,
        VelocityChange
    }

    public interface IParallelFixedUpdate
    {
        void ParallelFixedUpdate(Fix64 deltaTime);
    }

    public interface IParallelCollision2D
    {
        void OnParallelCollisionEnter2D(PCollision2D collision);
        void OnParallelCollisionStay2D(PCollision2D collision);
        void OnParallelCollisionExit2D(PCollision2D collision);
    }

    //TODO: which collider?
    public interface IParallelTrigger2D
    {
        void OnParallelTriggerEnter2D(ParallelRigidbody2D other);
        void OnParallelTriggerStay2D(ParallelRigidbody2D other);
        void OnParallelTriggerExit2D(ParallelRigidbody2D other);
    }

    public interface IParallelCollision3D
    {
        void OnParallelCollisionEnter3D(PCollision3D collision);
        void OnParallelCollisionStay3D(PCollision3D collision);
        void OnParallelCollisionExit3D(PCollision3D collision);
    }

    public interface IParallelTrigger3D
    {
        void OnParallelTriggerEnter3D(ParallelRigidbody3D other);
        void OnParallelTriggerStay3D(ParallelRigidbody3D other);
        void OnParallelTriggerExit3D(ParallelRigidbody3D other);
    }
}