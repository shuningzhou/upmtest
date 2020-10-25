using UnityEngine;
using System;

namespace SWNetwork.FrameSync
{
    public class StaticFrameSyncBehaviour : FrameSyncBehaviour
    {
        public UInt16 StaticFrameSyncBehaviourID;

        protected override void Awake()
        {
            base.Awake();
            _id = StaticFrameSyncBehaviourID;
            RegisterToFrameSyncBehaviourManager();
        }

        public void RegisterToFrameSyncBehaviourManager()
        {
            StaticFrameSyncBehaviourManager.Register(this);
        }
    }
}