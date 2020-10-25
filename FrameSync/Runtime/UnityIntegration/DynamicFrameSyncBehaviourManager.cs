using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWNetwork.Core;
using UnityEngine;

namespace SWNetwork.FrameSync
{
    public class DynamicFrameSyncBehaviourManager : SWFrameSyncSystem
    {
        public SortedList<UInt16, DynamicFrameSyncBehaviour> _behaviours;
        public List<DynamicFrameSyncBehaviour> _bufferedNewBehaviours;
        public List<DynamicFrameSyncBehaviour> _bufferedRemovedBehaviours;

        public UInt16 _nextDynamicBehaviourIndex;

        public DynamicFrameSyncBehaviourManager()
        {
            _behaviours = new SortedList<ushort, DynamicFrameSyncBehaviour>();
            _bufferedNewBehaviours = new List<DynamicFrameSyncBehaviour>();
            _bufferedRemovedBehaviours = new List<DynamicFrameSyncBehaviour>();
            _nextDynamicBehaviourIndex = 1;

            DynamicFrameSyncBehaviourSpawner.Init();
            DynamicFrameSyncBehaviourSpawner.OnGameObjectCreated += _behaviourSpawner_OnGameObjectCreated;
            DynamicFrameSyncBehaviourSpawner.OnGameObjectDestroyed += _behaviourSpawner_OnGameObjectDestroyed;
        }

        private void _behaviourSpawner_OnGameObjectDestroyed(GameObject gameObject)
        {
            DynamicFrameSyncBehaviour behaviour = gameObject.GetComponent<DynamicFrameSyncBehaviour>();

            if (behaviour == null)
            {
                throw new InvalidOperationException($"DynamicFrameSyncBehaviour component not found on GameObject {gameObject.name}.");
            }

            if (!_behaviours.ContainsValue(behaviour))
            {
                throw new InvalidOperationException($"Unmanaged DynamicFrameSyncBehaviour component found on GameObject {gameObject.name}.");
            }

            _bufferedRemovedBehaviours.Add(behaviour);
            behaviour.hasBufferedToRemove = true;
        }

        private void _behaviourSpawner_OnGameObjectCreated(GameObject gameObject, UInt16 prefabIndex)
        {
            DynamicFrameSyncBehaviour behaviour = gameObject.AddComponent<DynamicFrameSyncBehaviour>();
            behaviour._prefabIndex = prefabIndex;
            behaviour._id = _nextDynamicBehaviourIndex;
            _bufferedNewBehaviours.Add(behaviour);
            //SWConsole.Crit($"[NetworkEntityFrameSyncSystem] _networkEntitySpawner_OnGameObjectCreated 1  _nextEntityIndex={_nextEntityIndex}");
            _nextDynamicBehaviourIndex++;
            //SWConsole.Crit($"[NetworkEntityFrameSyncSystem] _networkEntitySpawner_OnGameObjectCreated 2  _nextEntityIndex={_nextEntityIndex}");
        }

        void Remove(DynamicFrameSyncBehaviour behaviour)
        {
            UInt16 id = behaviour.FrameSyncBehaviourID;

            _behaviours.Remove(id);

            DynamicFrameSyncBehaviourSpawner._Destroy(behaviour.gameObject);
        }

        void Remove(UInt16 id)
        {
            DynamicFrameSyncBehaviour behaviour = _behaviours[id];
            _behaviours.Remove(id);
            DynamicFrameSyncBehaviourSpawner._Destroy(behaviour.gameObject);
        }

        DynamicFrameSyncBehaviour Add(UInt16 index, UInt16 prefabId)
        {
            GameObject gameObject = DynamicFrameSyncBehaviourSpawner._Instantiate(prefabId);
            DynamicFrameSyncBehaviour behaviour = gameObject.AddComponent<DynamicFrameSyncBehaviour>();
            behaviour._prefabIndex = prefabId;
            behaviour._id = index;
            _behaviours[index] = behaviour;

            return behaviour;
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
            _nextDynamicBehaviourIndex = buffer.PopUInt16();
            UInt16 importBehaviourCount = buffer.PopUInt16();

            //index always starts from 1
            UInt16 importBehaviourIndex = 1;
            UInt16 selfBehaviourIndex = 1;

            UInt16 importBehaviourRead = 0;
            UInt16 selfBehaviourRead = 0;

            UInt16 selfKeyIndex = 0;

            UInt16 selfBehaviourCount = (UInt16)_behaviours.Count;

            if (selfBehaviourCount > 0)
            {
                selfBehaviourIndex = _behaviours.Keys[selfKeyIndex];
            }

            if(importBehaviourCount > 0)
            {
                importBehaviourIndex = buffer.PopUInt16();
            }

            bool shouldReadImport = false;
            bool shouldReadSelf = false;

            while (importBehaviourRead < importBehaviourCount && selfBehaviourRead < selfBehaviourCount)
            {
                if (shouldReadImport)
                {
                    importBehaviourIndex = buffer.PopUInt16();
                }

                if (shouldReadSelf)
                {
                    selfBehaviourIndex = _behaviours.Keys[selfKeyIndex];
                }

                if (importBehaviourIndex == selfBehaviourIndex)
                {
                    //id matched
                    DynamicFrameSyncBehaviour behaviour = _behaviours[selfBehaviourIndex];

                    //check prefab
                    UInt16 importPrefebId = buffer.PopUInt16();
                    UInt16 selfPrefabId = behaviour.prefabID;

                    if (importPrefebId == selfPrefabId)
                    {
                        //prefab matched
                        behaviour.ImportData(buffer);
                    }
                    else
                    {
                        //self behaviour was spawned with a different prefab

                        //remove self wrong GameObject
                        Remove(selfBehaviourIndex);

                        //create a new GameObject with the correct prefab
                        DynamicFrameSyncBehaviour newBehaviour = Add(importBehaviourIndex, importPrefebId);
                        newBehaviour.ImportData(buffer);
                    }

                    importBehaviourRead++;
                    selfBehaviourRead++;
                    selfKeyIndex++;

                    shouldReadImport = true;
                    shouldReadSelf = true;
                }
                else if (importBehaviourIndex > selfBehaviourIndex)
                {
                    //remove extra self behaviour
                    Remove(selfBehaviourIndex);

                    selfBehaviourRead++;

                    //read next self behaviour only
                    shouldReadImport = false;
                    shouldReadSelf = true;
                }
                else
                {
                    //add missing import entity
                    UInt16 importPrefabId = buffer.PopUInt16();
                    DynamicFrameSyncBehaviour newBehaviour = Add(importBehaviourIndex, importPrefabId);
                    newBehaviour.ImportData(buffer);

                    importBehaviourRead++;

                    //key just added, we want to skip the just added index
                    selfKeyIndex++;

                    //just increase key index, no need to read self behaviour
                    shouldReadImport = true;
                    shouldReadSelf = false;
                }
            }
            
            while(importBehaviourRead < importBehaviourCount)
            {
                if(shouldReadImport)
                {
                    importBehaviourIndex = buffer.PopUInt16();
                }

                UInt16 importPrefabId = buffer.PopUInt16();
                DynamicFrameSyncBehaviour newBehaviour = Add(importBehaviourIndex, importPrefabId);
                newBehaviour.ImportData(buffer);

                importBehaviourRead++;

                //read next remote behaviour
                shouldReadImport = true;
            }

            while(selfBehaviourRead < selfBehaviourCount)
            {
                if(shouldReadSelf)
                {
                    selfBehaviourIndex = _behaviours.Keys[selfKeyIndex];
                }

                Remove(selfBehaviourIndex);

                selfBehaviourRead++;

                //read next self behaviour
                shouldReadSelf = true;
            }
        }

        public override int Export(SWBytes buffer)
        {
            int size = 0;

            buffer.Push(_nextDynamicBehaviourIndex);
            size += 2;

            UInt16 behaviourCount = (UInt16)_behaviours.Count;
            buffer.Push(behaviourCount);
            size += 2;

            foreach(var pair in _behaviours)
            {
                UInt16 id = pair.Key;
                DynamicFrameSyncBehaviour behaviour = pair.Value;

                buffer.Push(id);
                size += 2;

                buffer.Push(behaviour.prefabID);
                size += 2;

                size += behaviour.ExportData(buffer);
            }

            return 0;
        }

        //


        //

        public override void Start()
        {
            SWConsole.Info("DynamicFrameSyncBehaviourManager: started");
        }

        public override void WillUpdate()
        {
            _bufferedNewBehaviours.Clear();
            _bufferedRemovedBehaviours.Clear();
        }

        public override void Update(FrameSyncGame game, FrameSyncInput input, FrameSyncUpdateType updateType)
        {
            foreach (var pair in _behaviours)
            {
                DynamicFrameSyncBehaviour behaviour = pair.Value;

                if(behaviour.hasBufferedToRemove)
                {
                    continue;
                }

                if (!behaviour._isInitialized)
                {
                    behaviour.InvokeFrameSyncDataInitialize(game);
                }

                behaviour.InvokeFrameSyncFixedUpdate(input, updateType);
            }

            foreach (DynamicFrameSyncBehaviour behaviour in _bufferedNewBehaviours)
            {
                if (!behaviour._isInitialized)
                {
                    behaviour.InvokeFrameSyncDataInitialize(game);
                }
                behaviour.InvokeFrameSyncFixedUpdate(input, updateType);

                _behaviours.Add(behaviour.FrameSyncBehaviourID, behaviour);
            }

            foreach(DynamicFrameSyncBehaviour behaviour in _bufferedRemovedBehaviours)
            {
                Remove(behaviour);
            }

            _bufferedNewBehaviours.Clear();
            _bufferedRemovedBehaviours.Clear();
        }

        public override void Stop()
        {
            SWConsole.Info("DynamicFrameSyncBehaviourManager: stopped");
            DynamicFrameSyncBehaviourSpawner.OnGameObjectCreated -= _behaviourSpawner_OnGameObjectCreated;
            DynamicFrameSyncBehaviourSpawner.OnGameObjectDestroyed -= _behaviourSpawner_OnGameObjectDestroyed;
        }
    }
}
