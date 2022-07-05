using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextMeshTest : MonoBehaviour
{
    private TextMeshProUGUI textMesh;
    // Start is called before the first frame update
    void Start()
    {
        textMesh = Camera.FindObjectOfType<TextMeshProUGUI>();
        textMesh.text = "Hej";
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("HALLØJ");
        // textMesh.textMeshPro.text = "Hej";
        textMesh.text = "Hej";
    }
}
