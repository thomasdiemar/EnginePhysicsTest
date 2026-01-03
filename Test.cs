using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

public class ThrusterInfo
{
    public int Index;
    public string CustomName;
    public Vector3 LocalPosition;
    public Vector3 MaxTorque;
    public Vector3 MaxForce;
    public float MaxCost;
    public float Mass;

    public ConstantForce CurrentForce;
    
    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.AppendLine(nameof(LocalPosition) + ":" + LocalPosition.ToString());
        builder.AppendLine(nameof(MaxTorque) + ":" + MaxTorque.ToString());
        builder.AppendLine(nameof(MaxForce) + ":" + MaxForce.ToString());
        return builder.ToString();
    }
}

public class Test : MonoBehaviour
{
    static Vector3 ControlThrustDirection = Vector3.zero;
    static Vector3 ControlTorqueDirection = Vector3.forward;
    static string filename = "Test_Unity_8_3_way_Engines_" + ControlThrustDirection.ToString() + "_" + ControlTorqueDirection.ToString();

    List<ThrusterInfo> thrusters = new List<ThrusterInfo>();

    void Start()
    {
        Debug.Log("FINDME");

        var maximalForce = new Dictionary<Vector3, double>
        {
            { Vector3.right, 0 },
            { Vector3.left, 0 },
            { Vector3.up, 0 },
            { Vector3.down, 0 },
            { Vector3.forward, 0 },
            { Vector3.back, 0 }
        };
        
        var maximalTorque = new Dictionary<Vector3, double>
        {
            { Vector3.right, 0 },
            { Vector3.left, 0 },
            { Vector3.up, 0 },
            { Vector3.down, 0 },
            { Vector3.forward, 0 },
            { Vector3.back, 0 }
        };

        var Capsule = gameObject;
        Debug.Assert(Capsule != null);
        var builder = new StringBuilder();

        var capsulerigidbody = Capsule.GetComponent<Rigidbody>();
        var shipcenterofmass = capsulerigidbody.centerOfMass;
        builder.AppendLine("Ship Center of Mass: " + shipcenterofmass.ToString());

        builder.AppendLine(Capsule.name + " - " + Capsule.GetType().ToString());

        var children = GetComponents<FixedJoint>();

        children.ToList().ForEach(x =>
        {
            // Process only the first 3 directions to avoid duplication
            var directions = new[] { Vector3.right, Vector3.up, Vector3.forward };
            foreach (var mf in directions)
            {
                var thruster = new ThrusterInfo();

                var connected = x.connectedBody;
                thruster.CustomName = connected.name + mf.ToString();

                var constantforce = connected.GetComponentInParent<ConstantForce>();
                var constantforce2 = mf * 100;
                thruster.MaxForce = constantforce2;

                thruster.CurrentForce = constantforce;
                thruster.CurrentForce.relativeForce = Vector3.zero;

                thruster.Mass = connected.mass;
                var relativeposition = constantforce.transform.localPosition;

                var transform = connected.GetComponentInParent<Transform>();
                thruster.LocalPosition = relativeposition;

                var r = thruster.LocalPosition - shipcenterofmass;
                builder.AppendLine("l: " + r.ToString());
                var T = Vector3.Cross(r, thruster.MaxForce);

                thruster.MaxTorque = T;
                thruster.MaxCost = 100;

                thrusters.Add(thruster);

                builder.Append(thruster.ToString());
            }
        });

        Debug.Log(builder.ToString());

        builder = new StringBuilder();
        builder.AppendLine("PhaseIPhaseIIGoalSolver");

        var variablecount = thrusters.Count;
        var maximalCost = 0.0;

        // Calculate maximum forces, torques and cost
        for (int i = 0; i < variablecount; i++)
        {
            // Update maximal force values
            if (thrusters[i].MaxForce.x > 0)
                maximalForce[Vector3.right] += thrusters[i].MaxForce.x;
            else
                maximalForce[Vector3.left] += thrusters[i].MaxForce.x;

            if (thrusters[i].MaxForce.y > 0)
                maximalForce[Vector3.up] += thrusters[i].MaxForce.y;
            else
                maximalForce[Vector3.down] += thrusters[i].MaxForce.y;

            if (thrusters[i].MaxForce.z > 0)
                maximalForce[Vector3.forward] += thrusters[i].MaxForce.z;
            else
                maximalForce[Vector3.back] += thrusters[i].MaxForce.z;

            // Update maximal torque values
            if (thrusters[i].MaxTorque.x > 0)
                maximalTorque[Vector3.right] += thrusters[i].MaxTorque.x;
            else
                maximalTorque[Vector3.left] += thrusters[i].MaxTorque.x;

            if (thrusters[i].MaxTorque.y > 0)
                maximalTorque[Vector3.up] += thrusters[i].MaxTorque.y;
            else
                maximalTorque[Vector3.down] += thrusters[i].MaxTorque.y;

            if (thrusters[i].MaxTorque.z > 0)
                maximalTorque[Vector3.forward] += thrusters[i].MaxTorque.z;
            else
                maximalTorque[Vector3.back] += thrusters[i].MaxTorque.z;

            // Update maximal cost
            maximalCost += thrusters[i].MaxCost;
        }

        // Priority settings - simplified logic
        int thrustpriorityactive = ControlTorqueDirection != Vector3.zero ? 3 : 4;
        int thrustpriorityinactive = ControlTorqueDirection != Vector3.zero ? 1 : 2;

        int torquepriorityactive = ControlTorqueDirection == Vector3.zero || ControlThrustDirection != Vector3.zero ? 3 : 4;
        int torquepriorityinactive = ControlTorqueDirection == Vector3.zero || ControlThrustDirection != Vector3.zero ? 1 : 2;

        int costpriority = thrustpriorityactive == 4 || torquepriorityactive == 4 ? 5 : 4;
        int thrusterpriority = costpriority + 1;

        // Setup solver variables
        var solverVariables = new List<SolverVariable>();
        for (int i = 0; i < variablecount; i++)
        {
            solverVariables.Add(new SolverVariable 
            {
                Name = thrusters[i].CustomName,
                Bound = new SolverRequestBound { Lower = 0, Upper = 1 },
                Goal = new SolverGoal { Priority = thrusterpriority, Minimize = true },
                Info = thrusters[i].LocalPosition.ToString()
            });
        }

        // Setup solver identifiers
        var solverIdentifiers = new List<SolverIdentifier>
        {
            new SolverIdentifier{
                Name="Fx",
                Bound = new SolverRequestBound { Lower = ControlThrustDirection.x < 0 ? (int)maximalForce[Vector3.left] : 0, Upper = ControlThrustDirection.x > 0 ? (int)maximalForce[Vector3.right] : 0 },
                Goal = new SolverGoal { Priority = ControlThrustDirection.x != 0 ? thrustpriorityactive : thrustpriorityinactive, Minimize = ControlThrustDirection.x <= 0 } },
            new SolverIdentifier{
                Name="Fy",
                Bound = new SolverRequestBound { Lower = ControlThrustDirection.y < 0 ? (int)maximalForce[Vector3.down] : 0, Upper = ControlThrustDirection.y > 0 ? (int)maximalForce[Vector3.up] : 0 },
                Goal = new SolverGoal { Priority = ControlThrustDirection.y != 0 ? thrustpriorityactive : thrustpriorityinactive, Minimize = ControlThrustDirection.y <= 0 } },
            new SolverIdentifier{
                Name="Fz", 
                Bound = new SolverRequestBound { Lower = ControlThrustDirection.z < 0 ? (int)maximalForce[Vector3.back] : 0, Upper = ControlThrustDirection.z > 0 ? (int)maximalForce[Vector3.forward] : 0 },
                Goal = new SolverGoal { Priority = ControlThrustDirection.z != 0 ? thrustpriorityactive : thrustpriorityinactive, Minimize = ControlThrustDirection.z <= 0 } },
            new SolverIdentifier{
                Name="Tx",
                Bound = new SolverRequestBound { Lower = ControlTorqueDirection.x < 0 ? (int)maximalTorque[Vector3.left] : 0, Upper = ControlTorqueDirection.x > 0 ? (int)maximalTorque[Vector3.right] : 0 },
                Goal = new SolverGoal { Priority = ControlTorqueDirection.x != 0 ? torquepriorityactive : torquepriorityinactive, Minimize = ControlTorqueDirection.x <= 0 } },
            new SolverIdentifier{
                Name="Ty",
                Bound = new SolverRequestBound { Lower = ControlTorqueDirection.y < 0 ? (int)maximalTorque[Vector3.down] : 0, Upper = ControlTorqueDirection.y > 0 ? (int)maximalTorque[Vector3.up] : 0 },
                Goal = new SolverGoal { Priority = ControlTorqueDirection.y != 0 ? torquepriorityactive : torquepriorityinactive, Minimize = ControlTorqueDirection.y <= 0 } },
            new SolverIdentifier{
                Name="Tz",
                Bound = new SolverRequestBound { Lower = ControlTorqueDirection.z < 0 ? (int)maximalTorque[Vector3.back] : 0, Upper = ControlTorqueDirection.z > 0 ? (int)maximalTorque[Vector3.forward] : 0 },
                Goal = new SolverGoal { Priority = ControlTorqueDirection.z != 0 ? torquepriorityactive : torquepriorityinactive, Minimize = ControlTorqueDirection.z <= 0 } },
            new SolverIdentifier{
                Name="Cost",
                Bound = new SolverRequestBound{ Lower = 0, Upper = (int)maximalCost },
                Goal = new SolverGoal{ Priority = costpriority, Minimize = true} }
        };

        // Create solver request
        var solverRequest = new SolverRequest(solverVariables, solverIdentifiers);

        // Set coefficients for the solver
        for (int i = 0; i < variablecount; i++)
        {
            solverRequest.Coefficients[i, 0] = (int)thrusters[i].MaxForce.x;
            solverRequest.Coefficients[i, 1] = (int)thrusters[i].MaxForce.y;
            solverRequest.Coefficients[i, 2] = (int)thrusters[i].MaxForce.z;
            solverRequest.Coefficients[i, 3] = (int)thrusters[i].MaxTorque.x;
            solverRequest.Coefficients[i, 4] = (int)thrusters[i].MaxTorque.y;
            solverRequest.Coefficients[i, 5] = (int)thrusters[i].MaxTorque.z;
            solverRequest.Coefficients[i, 6] = (int)thrusters[i].MaxCost;
        }

        // Write request and solve
        filename = TestUtil.WriteRequest(solverRequest, filename, "Unity");
        Debug.Log(filename);
        SolverResult solverResult = TestUtil.TestFile(filename, "Unity", false);

        // Calculate total force and torque
        Vector3 torque = Vector3.zero;
        Vector3 SumForce = Vector3.zero;

        builder = new StringBuilder();
        for (int i = 0; i < solverResult.Variables.Count; i++)
        {
            var force = solverResult.Variables[i].Value;
            builder.AppendLine(solverResult.Variables[i].Name + " " + force);

            var x = (thrusters[i].MaxForce.x * (float)force);
            var y = (thrusters[i].MaxForce.y * (float)force);
            var z = (thrusters[i].MaxForce.z * (float)force);
            var F = new Vector3(x, y, z);

            builder.AppendLine("F:" + F.ToString());

            var r = shipcenterofmass - thrusters[i].LocalPosition;
            var Tf = Vector3.Cross(F, r);
            builder.AppendLine("T:" + Tf.ToString());

            torque += Tf;
            SumForce += F;
        }

        builder.AppendLine("SumForce: " + SumForce.ToString());
        builder.AppendLine("SumTorque: " + torque.ToString());

        Debug.Log(builder.ToString());

        // Apply forces to thrusters
        var children2 = GetComponents<FixedJoint>();
        foreach (var joint in children2)
        {
            var connected = joint.connectedBody;
            var constantforce = connected.GetComponentInParent<ConstantForce>();
            
            // Find the corresponding variable value by name
            var variable = solverResult.Variables.FirstOrDefault(v => v.Name.Contains(connected.name));
            if (variable != null)
            {
                var multiplier = variable.Value;
                constantforce.relativeForce = new Vector3(
                    constantforce.relativeForce.x * (float)multiplier,
                    constantforce.relativeForce.y * (float)multiplier,
                    constantforce.relativeForce.z * (float)multiplier
                );
            }
        }
    }

    void Update()
    {
        // Empty update method
    }
}
