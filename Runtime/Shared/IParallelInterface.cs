using System;
using Parallel;
using UnityEngine;

namespace Parallel
{
    public interface IReplayable
    {
        void Save(UInt32 step);
        bool Load(UInt32 step);
    }

    public interface IParallelRigidbody2D
    {
        void OnParallelCollisionEnter(PCollision2D collision);
        void OnParallelCollisionStay(PCollision2D collision);
        void OnParallelCollisionExit(PCollision2D collision);

        void OnParallelTriggerEnter(IParallelRigidbody2D other);
        void OnParallelTriggerStay(IParallelRigidbody2D other);
        void OnParallelTriggerExit(IParallelRigidbody2D other);

        void OnTransformUpdated();
        void Step(Fix64 timeStep);
    }

    public interface IParallelRigidbody3D
    {
        void OnParallelCollisionEnter(PCollision3D collision);
        void OnParallelCollisionStay(PCollision3D collision);
        void OnParallelCollisionExit(PCollision3D collision);

        void OnParallelTriggerEnter(IParallelRigidbody3D other);
        void OnParallelTriggerStay(IParallelRigidbody3D other);
        void OnParallelTriggerExit(IParallelRigidbody3D other);
        void OnTransformUpdated();
        void Step(Fix64 timeStep);
    }

    //public interface IParallelCollision2D
    //{
    //    void ParallelOnCollision2D(PCollision2D collision);
    //}

    //public interface IParallelCollision3D
    //{
    //    void ParallelOnCollision3D(PCollision3D collision);
    //}
}