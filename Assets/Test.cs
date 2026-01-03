using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;


public class Test : MonoBehaviour
{
    void Start()
    {
       
        Debug.Log("FINDME");

    
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
         
             

                var connected = x.connectedBody;

                var constantforce = connected.GetComponentInParent<ConstantForce>();
               
            
                var relativeposition = constantforce.transform.localPosition;


                var transform = connected.GetComponentInParent<Transform>();
         
                var r = relativeposition - shipcenterofmass;
                builder.AppendLine("l: " + r.ToString());
                //var T = Vector3.Cross(r, thruster.MaxForce);

             
                //thrusters.Add(thruster);

       
        });




            //var r = shipcenterofmass - thrusters[i].LocalPosition;
            //var Tf = Vector3.Cross(F, r);
     
         

    }

    // Update is called once per frame
    void Update()
    {

    }
}
