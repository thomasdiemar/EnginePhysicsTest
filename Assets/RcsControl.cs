using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RCS;
using LinearSolver;

public class RcsControl : MonoBehaviour
{
    float thrustPower = 10f;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("FINDME");

        var Capsule = gameObject;
        Debug.Assert(Capsule != null);

        Debug.Log(Capsule.name + " - " + Capsule.GetType().ToString());
 
        var engines = Capsule.GetComponentsInChildren<FixedJoint>();
        var thrusters = new Dictionary<string,RcsThruster>();
        foreach (var engine in engines)
        {
            Debug.Log("Engine: " + engine.gameObject.name);
            var localPos = engine.transform.localPosition;
            Debug.Log(engine.gameObject.name+" Local position: " + localPos);
            var localBackward = -(engine.transform.localRotation * Vector3.forward);
            Debug.Log($"{engine.gameObject.name} backward: {localBackward}");

            localBackward *= thrustPower;

            thrusters.Add(
                engine.gameObject.name,
                new RcsThruster(
                    new RcsVector<int>((int)localPos.x, (int)localPos.y, (int)localPos.z),
                    new RcsVector<int>((int)localBackward.x, (int)localBackward.y, (int)localBackward.z)
                )
            );
        }

        var capsulerigidbody = Capsule.GetComponent<Rigidbody>();
        var shipcenterofmass = capsulerigidbody.centerOfMass;
        Debug.Log("Ship Center of Mass: " + shipcenterofmass.ToString());

        var engine = new RcsEngine(thrusters);
        engine.CenterOfMass = new RcsVector<Fraction>(new Fraction(shipcenterofmass.x), new Fraction(shipcenterofmass.y), new Fraction(shipcenterofmass.z));

        var optimiser = new RcsEngineOptimiser<LinearSolver.Custom.GoalProgramming.PreEmptive.BoundedInteger.Simplex.LexicographicGoalSolver>();

        var command = new RcsCommand(new RcsVector<Fraction>(0, 0, -1), new RcsVector<Fraction>());
        var result = optimiser.Optimise(engine, command).Last().Result;

        LogResult(Capsule.GetType().ToString(), result);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LogResult(string title, RcsEngineResult result)
    {
        Debug.Log($"=== Thruster Outputs ({title}) ===");
        foreach (var output in result.ThrusterOutputs.OrderBy(t => t.Key))
            Debug.Log($"{output.Key}: {output.Value:F6}");

        Debug.Log("n=== Resultant Force ===");
        Debug.Log($"Fx: {result.ResultantForce.X:F6}");
        Debug.Log($"Fy: {result.ResultantForce.Y:F6}");
        Debug.Log($"Fz: {result.ResultantForce.Z:F6}");

        Debug.Log("n=== Resultant Torque ===");
        Debug.Log($"Tx: {result.ResultantTorque.X:F6}");
        Debug.Log($"Ty: {result.ResultantTorque.Y:F6}");
        Debug.Log($"Tz: {result.ResultantTorque.Z:F6}");
    }
}
