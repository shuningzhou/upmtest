using UnityEditor;
using UnityEngine;
using Parallel;
using System.IO;

namespace Parallel.EditorTools
{
    public static class ParallelMenu
    {
        [MenuItem("GameObject/Parallel/2D/Physics Controller", false, 0)]
        public static void CreatePhysicsController2D()
        {
            var gameObject = new GameObject("ParallelPhysicsController", 
                typeof(ParallelPhysicsController2D));

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem("GameObject/Parallel/3D/Physics Controller", false, 0)]
        public static void CreatePhysicsController3D()
        {
            var gameObject = new GameObject("ParallelPhysicsController", 
                typeof(ParallelPhysicsController3D));

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem("GameObject/Parallel/3D/Sphere", false, 0)]
        public static void CreateSphere()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gameObject.name = "Sphere";

            GameObject.DestroyImmediate(gameObject.GetComponent<SphereCollider>());

            gameObject.AddComponent<ParallelTransform>();
            gameObject.AddComponent<ParallelRigidbody3D>();
            gameObject.AddComponent<ParallelSphereCollider>();

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem("GameObject/Parallel/3D/Cube", false, 0)]
        public static void CreateCube()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.name = "Cube";

            GameObject.DestroyImmediate(gameObject.GetComponent<BoxCollider>());

            gameObject.AddComponent<ParallelTransform>();
            gameObject.AddComponent<ParallelRigidbody3D>();
            gameObject.AddComponent<ParallelCubeCollider>();

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem("GameObject/Parallel/3D/Capsule", false, 0)]
        public static void CreateCapsule3D()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            gameObject.name = "Capsule";

            GameObject.DestroyImmediate(gameObject.GetComponent<CapsuleCollider>());

            gameObject.AddComponent<ParallelTransform>();
            gameObject.AddComponent<ParallelRigidbody3D>();
            gameObject.AddComponent<ParallelCapsuleCollider3D>();

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem("GameObject/Parallel/3D/Quad", false, 0)]
        public static void CreatePlane()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gameObject.name = "Quad";

            GameObject.DestroyImmediate(gameObject.GetComponent<MeshCollider>());

            gameObject.AddComponent<ParallelTransform>();
            gameObject.AddComponent<ParallelRigidbody3D>().bodyType = Parallel.BodyType.Static;
            gameObject.AddComponent<ParallelMeshCollider>();

            

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem("GameObject/Parallel/3D/Cylinder", false, 0)]
        public static void CreateCylinder()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gameObject.name = "Cylinder";

            GameObject.DestroyImmediate(gameObject.GetComponent<CapsuleCollider>());

            gameObject.AddComponent<ParallelTransform>();
            gameObject.AddComponent<ParallelRigidbody3D>();
            gameObject.AddComponent<ParallelConvexCollider3D>().UpdateVertsLimit(40);

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [MenuItem("GameObject/Parallel/3D/Plane", false, 0)]
        public static void CreateQuad()
        {
            GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            gameObject.name = "Plane";

            GameObject.DestroyImmediate(gameObject.GetComponent<MeshCollider>());

            gameObject.AddComponent<ParallelTransform>();
            gameObject.AddComponent<ParallelRigidbody3D>().bodyType = Parallel.BodyType.Static;
            gameObject.AddComponent<ParallelMeshCollider>();

            

            Selection.activeGameObject = gameObject;
            SceneView.FrameLastActiveSceneView();
        }

        [InitializeOnLoad]
        static class InstallGizmos
        {
            public static void Copy(string sourceDirectory, string targetDirectory)
            {
                DirectoryInfo diSource = new DirectoryInfo(sourceDirectory);
                DirectoryInfo diTarget = new DirectoryInfo(targetDirectory);

                CopyAll(diSource, diTarget);
            }

            public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
            {
                Directory.CreateDirectory(target.FullName);

                // Copy each file into the new directory.
                foreach (FileInfo fi in source.GetFiles())
                {
                    string path = Path.Combine(target.FullName, fi.Name);
                    Debug.Log($"Copying src={fi.Name}");
                    Debug.Log($"Copying dst={path}");
                    fi.CopyTo(path, true);
                }
            }

            static InstallGizmos()
            {
                string dstFile = Application.dataPath + "/Gizmos";

                if (!Directory.Exists(dstFile))
                    Directory.CreateDirectory(dstFile);

                dstFile = Application.dataPath + "/Gizmos/Parallel";

                bool mustUpdate = false;
                if (!Directory.Exists(dstFile))
                {
                    mustUpdate = true;
                    Directory.CreateDirectory(dstFile);
                }

                string kPackageRoot = "Packages/com.socketweaver.parallel";
                string path = Path.GetFullPath(kPackageRoot + "/Gizmos/Parallel");
                path = path.Replace('\\', '/'); // because of GetFullPath()
                int index = path.LastIndexOf("/Editor");
                if (index >= 0)
                path = path.Substring(0, index);
                if (path.Length > 0)
                path = Path.GetFullPath(path);  // stupid backslashes

                Debug.Log(path);

                var dstTime = Directory.GetCreationTime(dstFile);
                var srcTime = Directory.GetCreationTime(path);
                Debug.Log($"dstTime={dstTime}");
                Debug.Log($"srcTime={srcTime}");
                if(srcTime > dstTime || mustUpdate)
                {
                    Debug.Log("update icons");
                    
                    Copy(path, dstFile);
                }
                else{
                    Debug.Log("icons update-to-date");
                }
                
            }
        }
    }
}