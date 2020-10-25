using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWNetwork.Core;

namespace SWNetwork.FrameSync
{
    public class StaticFrameSyncBehaviourManager : SWFrameSyncSystem
    {
        public static SortedList<UInt16, StaticFrameSyncBehaviour> _behaviours = new SortedList<ushort, StaticFrameSyncBehaviour>();
        static int instanceCount = 0;

        public static void Register(StaticFrameSyncBehaviour staticFrameSyncBehaviour)
        {
            SWConsole.Info($"StaticFrameSyncBehaviourManager: register {staticFrameSyncBehaviour.name}");
            _behaviours.Add(staticFrameSyncBehaviour.FrameSyncBehaviourID, staticFrameSyncBehaviour);
        }

        public StaticFrameSyncBehaviourManager()
        {
            
        }

        public override bool Compare(SWFrameSyncSystem other)
        {
            return true;
        }

        public override int GenerateSystemDataHash()
        {
            return 0;
        }

        public override void Import(SWBytes buffer)
        {
            UInt16 behaviourCount = buffer.PopUInt16();
            if (behaviourCount != _behaviours.Count)
            {
                throw new Exception($"StaticFrameSyncBehaviourManager: Import importBehaviourCount={behaviourCount} behaviourCount={ _behaviours.Count}");
            }

            foreach (var pair in _behaviours)
            {
                UInt16 id = pair.Key;
                StaticFrameSyncBehaviour behaviour = pair.Value;

                UInt16 importId = buffer.PopUInt16();

                if (importId != id)
                {
                    throw new Exception($"StaticFrameSyncBehaviourManager: Import importID={importId} id={id}");
                }

                behaviour.ImportData(buffer);
            }
        }

        public override int Export(SWBytes buffer)
        {
            int size = 0;
            UInt16 behaviourCount = (UInt16)_behaviours.Count;
            buffer.Push(behaviourCount);
            size += 2;

            foreach(var pair in _behaviours)
            {
                UInt16 id = pair.Key;
                StaticFrameSyncBehaviour behaviour = pair.Value;

                buffer.Push(id);
                size += 2;

                size += behaviour.ExportData(buffer);
            }

            return size;
        }

        public override void Start()
        {
            SWConsole.Info("StaticFrameSyncBehaviourManager: started");
            instanceCount++;
        }

        public override void Update(FrameSyncGame game, FrameSyncInput input, FrameSyncUpdateType updateType)
        {
            foreach(var pair in _behaviours)
            {
                StaticFrameSyncBehaviour behaviour = pair.Value;
                if(!behaviour._isInitialized)
                {
                    behaviour.InvokeFrameSyncDataInitialize(game);
                }
                behaviour.InvokeFrameSyncFixedUpdate(input, updateType);
            }
        }

        public override void Stop()
        {
            _behaviours.Clear();
            SWConsole.Info("StaticFrameSyncBehaviourManager: stopped");
            instanceCount--;
        }
    }
}
