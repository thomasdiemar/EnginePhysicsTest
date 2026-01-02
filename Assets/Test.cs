using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using RCS;

//#region Helper Classes
public class VariableResult
{
    public string Name;
    public double Value;
}

public class SolverResult
{
    public SolverResult()
    {
        Variables = new List<VariableResult>();
    }
    public List<VariableResult> Variables { get; }
}
//#endregion Helper Classes
  
//#region Generic Solver
//public class SolverRequest
//{
//    public SolverRequest(List<SolverVariable> variables, List<SolverIdentifier> identifiers)
//    {
//        Variables = variables;
//        Identifiers = identifiers;
//        Coefficients = new double[variables.Count, identifiers.Count];
//    }
//    public List<SolverVariable> Variables { get; }  //Thrusters, bound er 0,1 (0-100% thrust)
//    public List<SolverIdentifier> Identifiers { get; } //Values in thruster
//    //Variable x Identifiers
//    public double[,] Coefficients { get; }
//}

//public class SolverVariable
//{
//    public string Name;
//    public SolverBound Bound;
//    public SolverGoal Goal;
//}
//public class SolverIdentifier : SolverVariable
//{

//}
//public class SolverBound
//{
//    public double Lower;
//    public double Upper;
//}
//public class SolverGoal
//{
//    public int Priority;
//    public bool Minimize;
//}

//public class VariableResult
//{
//    public string Name;
//    public double Value;
//}

//public class SolverResult
//{
//    public SolverResult(List<SolverVariable> variables)
//    {
//        Data = variables.Select(x => new VariableResult { Name = x.Name, Value = 0 }).ToList();
//    }
//    public List<VariableResult> Data { get; }
//    //Burde være Dict<navn(string)
//}

//public class Solver
//{
//    PhaseIPhaseIIGoalSolver solver;

//    SolverResult Result;

//    int[] variables;

//    public SolverResult GetResult() 
//    { 
//        var variablecount = Result.Data.Count;
//        for(var i = 0; i< variablecount; i++)
//        { 
//            Result.Data[i].Value = solver.GetValue(variables[i]);
//        }
//        return Result;
//    }

//    public Solver(SolverRequest solverRequest)
//    {
//        Guard(solverRequest);
//        solver = new PhaseIPhaseIIGoalSolver();
//        Result = new SolverResult(solverRequest.Variables);
//        Setup(solverRequest);
//    }
//    private void Guard(SolverRequest solverRequest)
//    {
//        if (solverRequest == null)
//        {

//        }

//        //if (!variables.All(x => x.Identifiers.Count == variables[0].Identifiers.Count))
//        //{

//        //}
//    }

//    public IEnumerable<Progress> Solve(ISolverLogger logger = null) => solver.Solve(logger);

//    void Setup(SolverRequest solverdata)
//    {
//        var variablecount = solverdata.Variables.Count;
//        variables = new int[variablecount];
//        for (int i = 0; i < variablecount; i++)
//        {
//            solver.AddVariable(solverdata.Variables[i].Name, out variables[i]);  
//            solver.SetBounds(variables[i], solverdata.Variables[i].Bound.Lower, solverdata.Variables[i].Bound.Upper);
//            solver.AddGoal(variables[i], solverdata.Variables[i].Goal.Priority, solverdata.Variables[i].Goal.Minimize);
//        }

//        var identifiercount = solverdata.Identifiers.Count;
//        var identifiers = new int[identifiercount];
//        for (int i = 0; i < identifiercount; i++)
//        {
//            solver.AddRow(solverdata.Identifiers[i].Name, out identifiers[i]);
//            solver.SetBounds(identifiers[i], solverdata.Identifiers[i].Bound.Lower, solverdata.Identifiers[i].Bound.Upper); 
//            solver.AddGoal(identifiers[i], solverdata.Identifiers[i].Goal.Priority, solverdata.Identifiers[i].Goal.Minimize);
//        }

//        for (int i = 0; i < variablecount; i++)
//        {
//            for (int j = 0; j < identifiercount; j++)
//            {
//                solver.SetCoefficient(identifiers[j], variables[i], solverdata.Coefficients[i,j]);  
//            }
//        }
//    }


//}

//#endregion Generic Solver

//#region Generic Engine
////EngineControl -> List<SolverVariable>


////public interface IMyThrust : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
////{
////    float ThrustOverride { get; set; }

////    float ThrustOverridePercentage { get; set; }

////    float MaxThrust { get; }

////    float MaxEffectiveThrust { get; }

////    float CurrentThrust { get; }

////    Vector3I GridThrustDirection { get; }
////}
////}
////public interface IMyGyro : IMyFunctionalBlock, IMyTerminalBlock, IMyCubeBlock, IMyEntity
////{
////    float GyroPower { get; set; }

////    bool GyroOverride { get; set; }

////    float Yaw { get; set; }

////    float Pitch { get; set; }

////    float Roll { get; set; }
////SetOrientation(Quaternoin)

//}
class ThrusterInfo
{
    //også id
    public int Index;
    public string CustomName;
    public Vector3 LocalPosition;
    public Vector3 MaxTorque;
    public Vector3 MaxForce;  // = MaxThrust * GridThrustDirection
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

//#endregion Generic Engine

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    //Solver solver;

    static Vector3 ControlThrustDirection = Vector3.zero;
    static Vector3 ControlTorqueDirection = Vector3.forward;
    static string filename = "Test_Unity_8_3_way_Engines_" + ControlThrustDirection.ToString() + "_" + ControlTorqueDirection.ToString();


    List<ThrusterInfo> thrusters = new List<ThrusterInfo>();

    Dictionary<Vector3, SolverResult> SolverResults = new Dictionary<Vector3, SolverResult>();

    void Start()
    {
       
        Debug.Log("FINDME");

        var maximalForce = new Dictionary<Vector3, double>
        {
            { Vector3.right, 0 },   //{1,0,0}
            { Vector3.left, 0 },    //{-1,0,0}
            { Vector3.up, 0 },      //{0,1,0}
            { Vector3.down, 0 },    //{0,-1,0}
            { Vector3.forward, 0 }, //{0,0,1}
            { Vector3.back, 0 }     //{0,0,-1}
        };
        var maximalTorque = new Dictionary<Vector3, double>
        {
            { Vector3.right, 0 },   //{1,0,0}
            { Vector3.left, 0 },    //{-1,0,0}
            { Vector3.up, 0 },      //{0,1,0}
            { Vector3.down, 0 },    //{0,-1,0}
            { Vector3.forward, 0 }, //{0,0,1}
            { Vector3.back, 0 }     //{0,0,-1}
        };

        //var Capsule = GetComponent<Component>();
        //var Capsule = GetComponent<GameObject>();
        var Capsule = gameObject;

        Debug.Assert(Capsule != null);
        var builder = new StringBuilder();

        var capsulerigidbody = Capsule.GetComponent<Rigidbody>();
        var shipcenterofmass = capsulerigidbody.centerOfMass;
        builder.AppendLine("Ship Center of Mass: " + shipcenterofmass.ToString());


        builder.AppendLine(Capsule.name + " - " + Capsule.GetType().ToString());
        //Capsule - UnityEngine.GameObject

        //var children = Capsule.GetComponentsInChildren<Component>(false);
        //var children = Capsule.GetComponentsInChildren<GameObject>(false);
        var children = GetComponents<FixedJoint>();

        // children = children.Where(x => x.tag == "Thruster").ToArray();
        var index = 0;
        children.ToList().ForEach(x =>
        {
            maximalForce.Keys.Take(3).ToList().ForEach(mf =>
            {
                var thruster = new ThrusterInfo();

                var connected = x.connectedBody;

                thruster.CustomName = connected.name + mf.ToString();

                var constantforce = connected.GetComponentInParent<ConstantForce>();
                var constantforce2 = mf * 100;
                //thruster.MaxForce = (constantforce.relativeForce); // * connected.mass;
                thruster.MaxForce = constantforce2; // * connected.mass;

                thruster.CurrentForce = constantforce;
                thruster.CurrentForce.relativeForce = Vector3.zero;


                thruster.Mass = connected.mass;
                var relativeposition = constantforce.transform.localPosition;


                var transform = connected.GetComponentInParent<Transform>();
                //builder.AppendLine("  " + transform.name + " - " + transform.GetType().ToString());
                //Cylinder 1 - UnityEngine.Transform
                //var relativeposition = transform.localPosition;

                //var relativeposition = connected.centerOfMass;
                //builder.AppendLine("Thruster relative position: " + relativeposition.ToString());

                thruster.LocalPosition = relativeposition;

                ///thruster.MaxForce = new Vector3(100f, 100f, 100f);
                //Distance Vector, r:
                //Force Vector, F:
                var r = thruster.LocalPosition - shipcenterofmass;
                builder.AppendLine("l: " + r.ToString());
                var T = Vector3.Cross(r, thruster.MaxForce);

                thruster.MaxTorque = T;
                thruster.MaxCost = 100;

                thrusters.Add(thruster);

                builder.Append(thruster.ToString());
            });
        });


        Debug.Log(builder.ToString());

        //////////////////////////////

        builder = new StringBuilder();
        builder.AppendLine("PhaseIPhaseIIGoalSolver");

        //var alignData = AlignData(data);
        var variablecount = thrusters.Count;

        
        var maximalCost = 0.0;

        for (int i = 0; i < variablecount; i++)
        {
            maximalForce[Vector3.right] += thrusters[i].MaxForce.x > 0.0 ? thrusters[i].MaxForce.x : 0.0;
            maximalForce[Vector3.left] += thrusters[i].MaxForce.x < 0.0 ? thrusters[i].MaxForce.x : 0.0;
            maximalForce[Vector3.up] += thrusters[i].MaxForce.y > 0.0 ? thrusters[i].MaxForce.y : 0.0;
            maximalForce[Vector3.down] += thrusters[i].MaxForce.y < 0.0 ? thrusters[i].MaxForce.y : 0.0;
            maximalForce[Vector3.forward] += thrusters[i].MaxForce.z > 0.0 ? thrusters[i].MaxForce.z : 0.0;
            maximalForce[Vector3.back] += thrusters[i].MaxForce.z < 0.0 ? thrusters[i].MaxForce.z : 0.0;

            maximalTorque[Vector3.right] += thrusters[i].MaxTorque.x > 0.0 ? thrusters[i].MaxTorque.x : 0.0;
            maximalTorque[Vector3.left] += thrusters[i].MaxTorque.x < 0.0 ? thrusters[i].MaxTorque.x : 0.0;
            maximalTorque[Vector3.up] += thrusters[i].MaxTorque.y > 0.0 ? thrusters[i].MaxTorque.y : 0.0;
            maximalTorque[Vector3.down] += thrusters[i].MaxTorque.y < 0.0 ? thrusters[i].MaxTorque.y : 0.0;
            maximalTorque[Vector3.forward] += thrusters[i].MaxTorque.z > 0.0 ? thrusters[i].MaxTorque.z : 0.0;
            maximalTorque[Vector3.back] += thrusters[i].MaxTorque.z < 0.0 ? thrusters[i].MaxTorque.z : 0.0;

            //maximalForce[Vector3.right] += thrusters[i].MaxForce.x ;
            //maximalForce[Vector3.left] += thrusters[i].MaxForce.x;
            //maximalForce[Vector3.up] += thrusters[i].MaxForce.y;
            //maximalForce[Vector3.down] -= thrusters[i].MaxForce.y;
            //maximalForce[Vector3.forward] -= thrusters[i].MaxForce.z ;
            //maximalForce[Vector3.back] -=  thrusters[i].MaxForce.z;

            //maximalTorque[Vector3.right] +=  thrusters[i].MaxTorque.x;
            //maximalTorque[Vector3.left] +=  thrusters[i].MaxTorque.x ;
            //maximalTorque[Vector3.up] +=  thrusters[i].MaxTorque.y ;
            //maximalTorque[Vector3.down] -=  thrusters[i].MaxTorque.y;
            //maximalTorque[Vector3.forward] -=  thrusters[i].MaxTorque.z;
            //maximalTorque[Vector3.back] -=  thrusters[i].MaxTorque.z ;

            //maximalCost +=
            //    thrusters[i].MaxCost > 0.0 &&
            //    (thrusters[i].MaxForce.x > 0 && ControlThrustDirection.x > 0) || (thrusters[i].MaxForce.x < 0 && ControlThrustDirection.x < 0) ||
            //    (thrusters[i].MaxForce.y > 0 && ControlThrustDirection.y > 0) || (thrusters[i].MaxForce.y < 0 && ControlThrustDirection.y < 0) ||
            //    (thrusters[i].MaxForce.z > 0 && ControlThrustDirection.z > 0) || (thrusters[i].MaxForce.z < 0 && ControlThrustDirection.z < 0) ||
            //    (thrusters[i].MaxForce.x > 0 && ControlTorqueDirection.y > 0) || (thrusters[i].MaxForce.x < 0 && ControlTorqueDirection.y < 0) ||
            //    (thrusters[i].MaxForce.y > 0 && ControlTorqueDirection.z > 0) || (thrusters[i].MaxForce.y < 0 && ControlTorqueDirection.z < 0) ||
            //    (thrusters[i].MaxForce.z > 0 && ControlTorqueDirection.x > 0) || (thrusters[i].MaxForce.z < 0 && ControlTorqueDirection.x < 0) ||
            //    (thrusters[i].MaxForce.x > 0 && ControlTorqueDirection.z > 0) || (thrusters[i].MaxForce.x < 0 && ControlTorqueDirection.z < 0) ||
            //    (thrusters[i].MaxForce.y > 0 && ControlTorqueDirection.x > 0) || (thrusters[i].MaxForce.y < 0 && ControlTorqueDirection.x < 0) ||
            //    (thrusters[i].MaxForce.z > 0 && ControlTorqueDirection.y > 0) || (thrusters[i].MaxForce.z < 0 && ControlTorqueDirection.y < 0)
            //    ? thrusters[i].MaxCost : 0.0;
            maximalCost += thrusters[i].MaxCost > 0.0 ? thrusters[i].MaxCost : 0 ;
        }

        // 4 active
        // 3 active
        // 2 inactive
        // 1 inactive

        //thust mindre når torque
        int thrustpriorityactive = ControlTorqueDirection != Vector3.zero ? 3 : 4; //foruden alle er sat og inaktive udgår
        int thrustpriorityinactive = ControlTorqueDirection != Vector3.zero ? 1 : 2;

        //torque mindre når thrust og ikke torque
        int torquepriorityactive = ControlTorqueDirection == Vector3.zero || ControlThrustDirection != Vector3.zero ? 3 : 4;
        int torquepriorityinactive = ControlTorqueDirection == Vector3.zero || ControlThrustDirection != Vector3.zero ? 1 : 2; //foruden alle er sat og inaktive udgår

        //int thrustpriorityactive = 4; //foruden alle er sat og inaktive udgår
        //int thrustpriorityinactive = 2;

        //torque mindre når thrust og ikke torque
        //int torquepriorityactive = 3;
        //int torquepriorityinactive = 1;

        int costpriority = thrustpriorityactive == 4 || torquepriorityactive == 4 ? 5 : 4;
        int thrusterpriority = costpriority + 1;

        // Create and configure RCS solver
        // Note: Assuming RCS provides a PhaseIPhaseIIGoalSolver compatible with the previous Common.LinearProgramming version
        dynamic solver = null;
        try
        {
            // Try to instantiate the solver from RCS
            var rcsType = Type.GetType("RCS.PhaseIPhaseIIGoalSolver, RCS");
            if (rcsType != null)
            {
                solver = Activator.CreateInstance(rcsType);
            }
            else
            {
                Debug.LogError("RCS.PhaseIPhaseIIGoalSolver not found. Available types in RCS need to be identified.");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Failed to create RCS solver: " + ex.Message);
            return;
        }

        var thrusterVariables = new int[variablecount];

        // Add thruster variables
        for (int i = 0; i < variablecount; i++)
        {
            solver.AddVariable(thrusters[i].CustomName, out thrusterVariables[i]);
            solver.SetBounds(thrusterVariables[i], 0, 1); // Thruster range 0-100%
            solver.AddGoal(thrusterVariables[i], thrusterpriority, true); // Minimize thrust usage
        }

        // Add constraint rows for forces and torques
        var constraintNames = new[] { "Fx", "Fy", "Fz", "Tx", "Ty", "Tz", "Cost" };
        var constraintBounds = new[]
        {
            new { Lower = ControlThrustDirection.x < 0 ? (int)maximalForce[Vector3.left] : 0, Upper = ControlThrustDirection.x > 0 ? (int)maximalForce[Vector3.right] : 0, Priority = ControlThrustDirection.x != 0 ? thrustpriorityactive : thrustpriorityinactive, Minimize = ControlThrustDirection.x <= 0 },
            new { Lower = ControlThrustDirection.y < 0 ? (int)maximalForce[Vector3.down] : 0, Upper = ControlThrustDirection.y > 0 ? (int)maximalForce[Vector3.up] : 0, Priority = ControlThrustDirection.y != 0 ? thrustpriorityactive : thrustpriorityinactive, Minimize = ControlThrustDirection.y <= 0 },
            new { Lower = ControlThrustDirection.z < 0 ? (int)maximalForce[Vector3.back] : 0, Upper = ControlThrustDirection.z > 0 ? (int)maximalForce[Vector3.forward] : 0, Priority = ControlThrustDirection.z != 0 ? thrustpriorityactive : thrustpriorityinactive, Minimize = ControlThrustDirection.z <= 0 },
            new { Lower = ControlTorqueDirection.x < 0 ? (int)maximalTorque[Vector3.left] : 0, Upper = ControlTorqueDirection.x > 0 ? (int)maximalTorque[Vector3.right] : 0, Priority = ControlTorqueDirection.x != 0 ? torquepriorityactive : torquepriorityinactive, Minimize = ControlTorqueDirection.x <= 0 },
            new { Lower = ControlTorqueDirection.y < 0 ? (int)maximalTorque[Vector3.down] : 0, Upper = ControlTorqueDirection.y > 0 ? (int)maximalTorque[Vector3.up] : 0, Priority = ControlTorqueDirection.y != 0 ? torquepriorityactive : torquepriorityinactive, Minimize = ControlTorqueDirection.y <= 0 },
            new { Lower = ControlTorqueDirection.z < 0 ? (int)maximalTorque[Vector3.back] : 0, Upper = ControlTorqueDirection.z > 0 ? (int)maximalTorque[Vector3.forward] : 0, Priority = ControlTorqueDirection.z != 0 ? torquepriorityactive : torquepriorityinactive, Minimize = ControlTorqueDirection.z <= 0 },
            new { Lower = 0, Upper = (int)maximalCost, Priority = costpriority, Minimize = true }
        };

        var constraints = new int[constraintNames.Length];
        for (int i = 0; i < constraintNames.Length; i++)
        {
            solver.AddRow(constraintNames[i], out constraints[i]);
            solver.SetBounds(constraints[i], constraintBounds[i].Lower, constraintBounds[i].Upper);
            solver.AddGoal(constraints[i], constraintBounds[i].Priority, constraintBounds[i].Minimize);
        }

        // Set coefficients: how each thruster contributes to each constraint
        for (int i = 0; i < variablecount; i++)
        {
            solver.SetCoefficient(constraints[0], thrusterVariables[i], (int)thrusters[i].MaxForce.x);    // Fx
            solver.SetCoefficient(constraints[1], thrusterVariables[i], (int)thrusters[i].MaxForce.y);    // Fy
            solver.SetCoefficient(constraints[2], thrusterVariables[i], (int)thrusters[i].MaxForce.z);    // Fz
            solver.SetCoefficient(constraints[3], thrusterVariables[i], (int)thrusters[i].MaxTorque.x);   // Tx
            solver.SetCoefficient(constraints[4], thrusterVariables[i], (int)thrusters[i].MaxTorque.y);   // Ty
            solver.SetCoefficient(constraints[5], thrusterVariables[i], (int)thrusters[i].MaxTorque.z);   // Tz
            solver.SetCoefficient(constraints[6], thrusterVariables[i], (int)thrusters[i].MaxCost);       // Cost
        }

        // Solve the linear programming problem
        var solveStatuses = solver.Solve();
        foreach (var status in solveStatuses)
        {
            Debug.Log("Solver: " + status);
        }

        // Extract results
        var solverResult = new SolverResult();
        for (int i = 0; i < variablecount; i++)
        {
            var thrustValue = solver.GetValue(thrusterVariables[i]);
            solverResult.Variables.Add(new VariableResult { Name = thrusters[i].CustomName, Value = thrustValue });
        }


        
        ///////////

        Vector3 torque = new Vector3();
        Vector3 SumForce = new Vector3();

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


        ///do pr profile
        var children2 = GetComponents<FixedJoint>();

        //children2.ToList().ForEach(x =>
        //{
        //    var connected = x.connectedBody;
        //    var constantforce = connected.GetComponentInParent<ConstantForce>();
        //    //constantforce.relativeForce =
        //    var multiplier = solverResult.Variables.Where(x => x.Name == connected.name).First().Value;
        //    constantforce.relativeForce = new Vector3(constantforce.relativeForce.x * (float)multiplier, constantforce.relativeForce.y * (float)multiplier, constantforce.relativeForce.z * (float)multiplier);
        //});
    }

    // Update is called once per frame
    void Update()
    {

    }
}
