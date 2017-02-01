using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WizardryConnect.W8;

public class TestReader : MonoBehaviour {

	// Use this for initialization
	void Start () {

        var slf = new SLFFile(@"W8\Data\DATA.SLF");
        var itemstr = @"ITEMS3D\BOOKFOUR.ITM";
        var data = slf.GetFile(itemstr);

       // var file2 = new ItemsFile(@"W8\Data\ITEMS3D\JACKHAMMER.ITM");
        var file = new ItemsFile(data, System.IO.Path.GetFileNameWithoutExtension(itemstr));
        var go = new GameObject();

        go.name = file.Name;

        var meshfilter=go.AddComponent<MeshFilter>();

        meshfilter.sharedMesh = file.ItemMesh;

        var meshRend = go.AddComponent<MeshRenderer>();
        meshRend.sharedMaterial = file.ItemMaterials[0];
        meshRend.materials = file.ItemMaterials;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
