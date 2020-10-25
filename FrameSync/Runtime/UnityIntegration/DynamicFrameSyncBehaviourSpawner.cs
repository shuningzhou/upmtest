using UnityEngine;
using System;

namespace SWNetwork.FrameSync
{
    public delegate void GameObjectCreated(GameObject gameObject, UInt16 prefabIndex);
    public delegate void GameObjectDestroyed(GameObject gameObject);

    public static class DynamicFrameSyncBehaviourSpawner
    {
        public static event GameObjectCreated OnGameObjectCreated;
        public static event GameObjectDestroyed OnGameObjectDestroyed;

        static GameObject[] _prefabList;
        static bool _initialized = false;

        public static void Init()
        {
            if(!_initialized)
            {
                _initialized = true;
                var prefabs = Resources.LoadAll<GameObject>("SWPrefabs");
                _prefabList = prefabs;
            }

        }

        public static void Init(GameObject[] prefabList)
        {
            if (!_initialized)
            {
                _initialized = true;
                _prefabList = prefabList;
            }
        }

        public static GameObject Instantiate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            int prefabIndex = Array.IndexOf(_prefabList, prefab);

            if (prefabIndex >= 0)
            {
                GameObject gameObject = GameObject.Instantiate(prefab, position, rotation);

                OnGameObjectCreated(gameObject, (UInt16)prefabIndex);

                return gameObject;
            }
            else
            {
                throw new ArgumentException("NetworkEntityManager", $"{prefab} is not in the SWPrefabs folder.");
            }
        }

        public static void Destroy(GameObject gameObject)
        {
            OnGameObjectDestroyed(gameObject);
        }

        internal static GameObject _Instantiate(UInt16 prefabIndex)
        {
            GameObject gameObject = GameObject.Instantiate(_prefabList[(int)prefabIndex]);

            return gameObject;
        }

        internal static void _Destroy(GameObject gameObject)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
