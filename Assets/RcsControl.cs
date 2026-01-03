using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RCS;
using LinearSolver;
using LinearSolver.Custom.GoalProgramming;

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
        var thrusters = new Dictionary<string, RcsThruster>();
        foreach (var engineJoint in engines)
        {
            var localPos = engineJoint.transform.localPosition;
            var localBackward = -(engineJoint.transform.localRotation * Vector3.forward);
            localBackward *= thrustPower;

            thrusters.Add(
                engineJoint.gameObject.name,
                new RcsThruster(
                    new RcsVector<int>((int)Mathf.Round(localBackward.x), (int)Mathf.Round(localBackward.y), (int)Mathf.Round(localBackward.z)),
                    new RcsVector<int>((int)Mathf.Round(localPos.x), (int)Mathf.Round(localPos.y), (int)Mathf.Round(localPos.z))
                )
            );
        }

        foreach (var thruster in thrusters)
        {
            Debug.Log($"Thruster {thruster.Key}: Position {thruster.Value.Position.X},{thruster.Value.Position.Y},{thruster.Value.Position.Z}, Direction {thruster.Value.Direction.X},{thruster.Value.Direction.Y},{thruster.Value.Direction.Z}");
        }

        var capsulerigidbody = Capsule.GetComponent<Rigidbody>();
        var shipcenterofmass = capsulerigidbody.centerOfMass;

        var engine = new RcsEngine(thrusters);

        var centerOfMassInt = new RcsVector<int>(
            (int)Mathf.Round(shipcenterofmass.x),
            (int)Mathf.Round(shipcenterofmass.y),
            (int)Mathf.Round(shipcenterofmass.z)
        );
        engine.CenterOfMass = centerOfMassInt;
        Debug.Log($"Engine Center of Mass: {engine.CenterOfMass.X},{engine.CenterOfMass.Y},{engine.CenterOfMass.Z} ");
        
        var optimiser = new RcsEngineOptimiser<LinearSolver.Custom.GoalProgramming.PreEmptive.BoundedInteger.Simplex.LexicographicGoalSolver>();
        var command = new RcsCommand(new RcsVector<Fraction>(0, 0, -1), new RcsVector<Fraction>(0,0,0));
        var result = optimiser.Optimise(engine, command).ToList().Last().Result;
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

        Debug.Log("=== Resultant Force ===");
        Debug.Log($"Fx: {result.ResultantForce.X:F6}");
        Debug.Log($"Fy: {result.ResultantForce.Y:F6}");
        Debug.Log($"Fz: {result.ResultantForce.Z:F6}");

        Debug.Log("=== Resultant Torque ===");
        Debug.Log($"Tx: {result.ResultantTorque.X:F6}");
        Debug.Log($"Ty: {result.ResultantTorque.Y:F6}");
        Debug.Log($"Tz: {result.ResultantTorque.Z:F6}");
    }
}



