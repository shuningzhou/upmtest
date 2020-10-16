using UnityEditor;
using UnityEngine;
using Parallel;

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
    }
}