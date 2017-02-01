#define KEEP_PREFAB_LINKS

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
namespace WizardryConnect.Utility
{

    public static class GameObjectHelper
    {
        /// <summary>
        /// Adds a single WizardryMesh game object to scene.
        /// </summary>
        /// <param name="modelName">ModelName of mesh to add.</param>
        /// <param name="parent">Optional parent of this object.</param>
        /// <param name="makeStatic">Flag to set object static flag.</param>
        /// <param name="useExistingObject">Add mesh to existing object rather than create new.</param>
        /// <param name="ignoreCollider">Force disable collider.</param>
        /// <returns>GameObject.</returns>
        public static GameObject CreateWizardryMeshGameObject(
            string modelName,
            Transform parent,
            bool makeStatic = false,
            GameObject useExistingObject = null,
            bool ignoreCollider = false)
        {
            Wizardry8Unity w8Unity = Wizardry8Unity.Instance;

            // Create gameobject
            string name = string.Format("Wizardry8Mesh [Name={0}]", modelName);
            GameObject go = (useExistingObject != null) ? useExistingObject : new GameObject();
            if (parent != null)
                go.transform.parent = parent;
            go.name = name;


            // Get mesh filter and renderer components
            MeshFilter meshFilter = go.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = go.GetComponent<MeshRenderer>();

            if (!meshFilter)
            {
                meshFilter = go.AddComponent<MeshFilter>();
            }
            if (!meshRenderer)
            {
                meshRenderer = go.AddComponent<MeshRenderer>();
            }

            Mesh ItemMesh = new Mesh();
            Material[] ItemMaterials = new Material[0];
            w8Unity.ContentReader.GetItem3D(
                modelName,
                out ItemMesh,
                out ItemMaterials
               );

            if (ItemMesh)
            {
                meshFilter.sharedMesh = ItemMesh;
                meshRenderer.sharedMaterials = ItemMaterials;
            }

            // Assign static
            if (makeStatic)
                go.isStatic = true;

            return go;
        }

    }
}
