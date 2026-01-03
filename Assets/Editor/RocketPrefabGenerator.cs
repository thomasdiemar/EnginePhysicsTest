using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class RocketPrefabGenerator : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create Rocket Prefab")]
    public static void CreateRocketPrefab()
    {
        // Create main rocket body
        GameObject rocket = new GameObject("Rocket");
        rocket.AddComponent<Rigidbody>();
        Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();
        rocketRb.mass = 1000f;

        // Create 4 rocket engines positioned around the rocket
        Vector3[] enginePositions = new Vector3[]
        {
            new Vector3(-1, -1, 0),   // Front-left
            new Vector3(1, -1, 0),   // Front-right
            new Vector3(-1, 1, 0),   // Back-left
            new Vector3(1, 1, 0)     // Back-right
        };

        for (int i = 0; i < 4; i++)
        {
            // Create engine GameObject
            GameObject engine = new GameObject($"Engine_{i}");
            engine.transform.SetParent(rocket.transform);
            engine.transform.localPosition = enginePositions[i];

            // Add Rigidbody to engine
            Rigidbody engineRb = engine.AddComponent<Rigidbody>();
            engineRb.mass = 10f;
            engineRb.isKinematic = false;

            // Add FixedJoint to connect engine to rocket body
            FixedJoint joint = engine.AddComponent<FixedJoint>();
            joint.connectedBody = rocketRb;

            // Add a visual cube to represent the engine
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(engine.transform);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.localScale = new Vector3(0.3f, 0.3f, 0.5f);
            
            // Remove collider from visual cube
            Collider cubeCollider = cube.GetComponent<Collider>();
            if (cubeCollider != null) DestroyImmediate(cubeCollider);
            
            // Remove default rigidbody from primitive
            Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb != null) DestroyImmediate(cubeRb);

            // Add ConstantForce for thrust simulation
            ConstantForce force = engine.AddComponent<ConstantForce>();
            force.force = Vector3.zero;
        }

        // Add visual representation to rocket body
        GameObject rocketVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rocketVisual.transform.SetParent(rocket.transform);
        rocketVisual.transform.localPosition = Vector3.zero;
        rocketVisual.transform.localScale = new Vector3(2f, 3f, 2f);
        
        // Remove collider from visual capsule
        Collider capsuleCollider = rocketVisual.GetComponent<Collider>();
        if (capsuleCollider != null) DestroyImmediate(capsuleCollider);
        
        // Remove default rigidbody from primitive
        Rigidbody visualRb = rocketVisual.GetComponent<Rigidbody>();
        if (visualRb != null) DestroyImmediate(visualRb);

        // Save as prefab
        string prefabPath = "Assets/Rocket.prefab";
        PrefabUtility.SaveAsPrefabAsset(rocket, prefabPath);
        
        Debug.Log($"Rocket prefab created and saved to {prefabPath}");

        // Destroy the temporary GameObject
        DestroyImmediate(rocket);
    }
#endif
}
