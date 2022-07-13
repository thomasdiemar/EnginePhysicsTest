using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using Common.LinearProgramming;
using Common.LinearProgramming.TestTools;

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

    static Vector3 ControlThrustDirection = Vector3.up;
    static Vector3 ControlTorqueDirection = Vector3.right;
    static string filename = "Test_Unity_12_Engines_" + ControlThrustDirection.ToString() + "_" + ControlTorqueDirection.ToString();


    List<ThrusterInfo> thrusters = new List<ThrusterInfo>();

    Dictionary<Vector3, SolverResult> SolverResults = new Dictionary<Vector3, SolverResult>();

    void Start()
    {
       
        Debug.Log("FINDME");


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

        children.ToList().ForEach(x =>
        {
            var thruster = new ThrusterInfo();
            //builder.AppendLine("  " + x.name + " - " + x.GetType().ToString());
            //Capsule - UnityEngine.FixedJoint

            var connected = x.connectedBody;
            //builder.AppendLine("  " + connected.name + " - " + connected.GetType().ToString());
            //Cylinder 1 - UnityEngine.Rigidbody
            //var thrustercenterofmass = connected.centerOfMass;
            //builder.AppendLine("Thruster Center of Mass: " + thrustercenterofmass.ToString());

            thruster.CustomName = connected.name;



            var constantforce = connected.GetComponentInParent<ConstantForce>();
            thruster.MaxForce = (constantforce.relativeForce); // * connected.mass;
            thruster.CurrentForce = constantforce;
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


        Debug.Log(builder.ToString());

        //////////////////////////////

        builder = new StringBuilder();
        builder.AppendLine("PhaseIPhaseIIGoalSolver");

        //var alignData = AlignData(data);
        var variablecount = thrusters.Count;

        int[] variables = new int[variablecount];

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


        var solverVariables = new List<SolverVariable>();
        for (int i = 0; i < variablecount; i++)
        {
            solverVariables.Add(new SolverVariable {
                Name = thrusters[i].CustomName,
                Bound = new SolverRequestBound { Lower = 0, Upper = 1 }, //Er 1 upperbound rigtig?
                Goal = new SolverGoal { Priority = thrusterpriority, Minimize = true },
                Info = thrusters[i].LocalPosition.ToString()
            }); //Brug så meget thrust som muligt (skal det have større prio?
                
        }



        var solverIdentifiers = new List<SolverIdentifier>
        {
            new SolverIdentifier{
                Name="Fx",
                Bound = new SolverRequestBound { Lower = ControlThrustDirection.x < 0 ? (int)maximalForce[Vector3.left] : 0, Upper = ControlThrustDirection.x > 0 ? (int)maximalForce[Vector3.right] : 0 },
                Goal = new SolverGoal { Priority = ControlThrustDirection.x != 0 ? thrustpriorityactive : thrustpriorityinactive, Minimize = ControlThrustDirection.x <= 0 } },
            new SolverIdentifier{
                Name="Fy",
                //Bound = new SolverRequestBound { Lower = maximalForce[Vector3.down], Upper = maximalForce[Vector3.up]  },
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
                Goal = new SolverGoal{ Priority = costpriority, Minimize = true} } //Betal lavest pris for energien
        };

        //var solverIdentifiers = new List<SolverIdentifier>
        //{
        //    new SolverIdentifier{
        //        Name="Fx",
        //        Bound = new SolverBound { Lower = ControlThrustDirection.x != 0 ? maximalForce[Vector3.left] : 0, Upper = ControlThrustDirection.x != 0 ?  maximalForce[Vector3.right] : 0 },
        //        Goal = new SolverGoal { Priority = ControlThrustDirection.x != 0 ? 1 : 2, Minimize = ControlThrustDirection.x <= 0 } },
        //    new SolverIdentifier{
        //        Name="Fy",
        //        //Bound = new SolverBound { Lower = maximalForce[Vector3.down], Upper = maximalForce[Vector3.up]  },
        //        Bound = new SolverBound { Lower = ControlThrustDirection.y != 0 ? maximalForce[Vector3.down] : 0, Upper = ControlThrustDirection.y != 0 ? maximalForce[Vector3.up] : 0 },
        //        Goal = new SolverGoal { Priority = ControlThrustDirection.y != 0 ? 1 : 2, Minimize = ControlThrustDirection.y <= 0 } },
        //    new SolverIdentifier{
        //        Name="Fz",
        //        Bound = new SolverBound { Lower = ControlThrustDirection.z != 0 ? maximalForce[Vector3.back] : 0, Upper = ControlThrustDirection.z != 0 ? maximalForce[Vector3.forward] : 0 },
        //        Goal = new SolverGoal { Priority = ControlThrustDirection.z != 0 ? 1 : 2, Minimize = ControlThrustDirection.z <= 0 } },
        //    new SolverIdentifier{
        //        Name="Tx",
        //        Bound = new SolverBound { Lower = ControlTorqueDirection.x != 0 ? maximalTorque[Vector3.left] : 0, Upper = ControlTorqueDirection.x != 0 ? maximalTorque[Vector3.right] : 0 },
        //        Goal = new SolverGoal { Priority = ControlTorqueDirection.x != 0 ? 1 : 2, Minimize = ControlTorqueDirection.x <= 0 } },
        //    new SolverIdentifier{
        //        Name="Ty",
        //        Bound = new SolverBound { Lower = ControlTorqueDirection.y != 0 ? maximalTorque[Vector3.down] : 0, Upper = ControlTorqueDirection.y != 0 ? maximalTorque[Vector3.up] : 0 },
        //        Goal = new SolverGoal { Priority = ControlTorqueDirection.y != 0 ? 1 : 2, Minimize = ControlTorqueDirection.y <= 0 } },
        //    new SolverIdentifier{
        //        Name="Tz",
        //        Bound = new SolverBound { Lower = ControlTorqueDirection.z != 0 ? maximalTorque[Vector3.back] : 0, Upper = ControlTorqueDirection.z != 0 ? maximalTorque[Vector3.forward] : 0 },
        //        Goal = new SolverGoal { Priority = ControlTorqueDirection.z != 0 ? 1 : 2, Minimize = ControlTorqueDirection.z <= 0 } },
        //    new SolverIdentifier{
        //        Name="Cost",
        //        Bound = new SolverBound{ Lower = 0, Upper = maximalCost },
        //        Goal = new SolverGoal{ Priority = 3, Minimize = true} } //Betal lavest pris for energien
        //};

        //Dette virker i microsoft solver,
        //var solverIdentifiers = new List<SolverIdentifier>
        //{
        //    new SolverIdentifier{
        //        Name="Fx",
        //        Bound = new SolverBound { Lower = maximalForce[Vector3.left], Upper =  maximalForce[Vector3.right] },
        //        Goal = new SolverGoal { Priority = ControlThrustDirection.x != 0 ? 1 : 2, Minimize = ControlThrustDirection.x <= 0 } },
        //    new SolverIdentifier{
        //        Name="Fy",
        //        Bound = new SolverBound { Lower = maximalForce[Vector3.down], Upper = maximalForce[Vector3.up]  },
        //        Goal = new SolverGoal { Priority = ControlThrustDirection.y != 0 ? 1 : 2, Minimize = ControlThrustDirection.y <= 0 } },
        //    new SolverIdentifier{
        //        Name="Fz",
        //        Bound = new SolverBound { Lower = maximalForce[Vector3.back] , Upper = maximalForce[Vector3.forward] },
        //        Goal = new SolverGoal { Priority = ControlThrustDirection.z != 0 ? 1 : 2, Minimize = ControlThrustDirection.z <= 0 } },
        //    new SolverIdentifier{
        //        Name="Tx",
        //        Bound = new SolverBound { Lower = maximalTorque[Vector3.left], Upper = maximalTorque[Vector3.right] },
        //        Goal = new SolverGoal { Priority = ControlTorqueDirection.x != 0 ? 1 : 2, Minimize = ControlTorqueDirection.x <= 0 } },
        //    new SolverIdentifier{
        //        Name="Ty",
        //        Bound = new SolverBound { Lower =  maximalTorque[Vector3.down] , Upper = maximalTorque[Vector3.up]  },
        //        Goal = new SolverGoal { Priority = ControlTorqueDirection.y != 0 ? 1 : 2, Minimize = ControlTorqueDirection.y <= 0 } },
        //    new SolverIdentifier{
        //        Name="Tz",
        //        Bound = new SolverBound { Lower =  maximalTorque[Vector3.back] , Upper = maximalTorque[Vector3.forward] },
        //        Goal = new SolverGoal { Priority = ControlTorqueDirection.z != 0 ? 1 : 2, Minimize = ControlTorqueDirection.z <= 0 } },
        //    new SolverIdentifier{
        //        Name="Cost",
        //        Bound = new SolverBound{ Lower = 0, Upper = maximalCost },
        //        Goal = new SolverGoal{ Priority = 3, Minimize = true} }
        //};

        var solverRequest = new SolverRequest
        (
            solverVariables,
            solverIdentifiers
        );

        for (int i = 0; i < variablecount; i++)
        {
            solverRequest.Coefficients[i, 0] = (int)thrusters[i].MaxForce.x;
            solverRequest.Coefficients[i, 1] = (int)thrusters[i].MaxForce.y;// * -1; //Når der minimeres vendes coefficenten (og hvad mere skal der ske?)
            solverRequest.Coefficients[i, 2] = (int)thrusters[i].MaxForce.z;
            solverRequest.Coefficients[i, 3] = (int)thrusters[i].MaxTorque.x;
            solverRequest.Coefficients[i, 4] = (int)thrusters[i].MaxTorque.y;
            solverRequest.Coefficients[i, 5] = (int)thrusters[i].MaxTorque.z;
            solverRequest.Coefficients[i, 6] = (int)thrusters[i].MaxCost;
        }

        //////

        //string jsonString = JsonConvert.SerializeObject(solverRequest);
        //Debug.Log(jsonString);

        //solver = new Solver(solverRequest);
        //var logger = new SolverLogger();

        //foreach (var status in solver.Solve(logger))
        //{
        //}

        //logger.Save("Unity_Simplex" + ".html");

        //var solverResult = solver.GetResult();
        SolverResult solverResult = null;
        //try
        //{
            filename = TestUtil.WriteRequest(solverRequest, filename, "Unity");

        Debug.Log(filename);
        solverResult = TestUtil.TestFile(filename, "Unity", false);
        //}
        //catch
        //{

        //}


        
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

        children2.ToList().ForEach(x =>
        {
            var connected = x.connectedBody;
            var constantforce = connected.GetComponentInParent<ConstantForce>();
            //constantforce.relativeForce =
            var multiplier = solverResult.Variables.Where(x => x.Name == connected.name).First().Value;
            constantforce.relativeForce = new Vector3(constantforce.relativeForce.x * (float)multiplier, constantforce.relativeForce.y * (float)multiplier, constantforce.relativeForce.z * (float)multiplier);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
