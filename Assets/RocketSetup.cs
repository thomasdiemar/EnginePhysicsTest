using UnityEngine;

public class RocketSetup : MonoBehaviour
{
    [ContextMenu("Create Rocket with Engines")]
    public void CreateRocket()
    {
        // Create main rocket body
        GameObject rocket = new GameObject("Rocket");
        rocket.AddComponent<Rigidbody>();
        Rigidbody rocketRb = rocket.GetComponent<Rigidbody>();
        rocketRb.mass = 1000f;
        rocketRb.constraints = RigidbodyConstraints.FreezeRotation; // or remove for full physics

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
            Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb != null) DestroyImmediate(cubeRb); // Remove default rigidbody from primitive
            
            // Add ConstantForce for thrust simulation
            ConstantForce force = engine.AddComponent<ConstantForce>();
            force.force = Vector3.zero; // Will be controlled by script

            Debug.Log($"Created Engine {i} at position {enginePositions[i]}");
        }

        // Add visual representation to rocket body
        GameObject rocketVisual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rocketVisual.transform.SetParent(rocket.transform);
        rocketVisual.transform.localPosition = Vector3.zero;
        rocketVisual.transform.localScale = new Vector3(2f, 3f, 2f);
        Rigidbody visualRb = rocketVisual.GetComponent<Rigidbody>();
        if (visualRb != null) DestroyImmediate(visualRb); // Remove default rigidbody from primitive

        Debug.Log("Rocket created with 4 engines connected via FixedJoint!");
    }
}
