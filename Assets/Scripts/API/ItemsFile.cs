
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using WizardryConnect.Utility;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;

namespace WizardryConnect.W8
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WZItemHeader
    {
        byte bt;
        public int nType;
        public int nVertices;
        public int nFaces;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct D3DWizVertex
    {
        public float x;
        public float y;
        public float z;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct D3DWizVertexSmall
    {
        public short x;
        public short y;
        public short z;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct D3DWizFace
    {
        public int nVertex1;
        public int nVertex2;
        public int nVertex3;
        public float ftu1, ftv1, ftu2, ftv2, ftu3, ftv3;
        public int nMaterial;
        public byte bt;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct D3DWizFaceSmall
    {
        public short shVertex1;
        public short shVertex2;
        public short shVertex3;
        public float ftu1, ftv1, ftu2, ftv2, ftu3, ftv3;
        public byte btMaterial;
        public byte bt1;
        public byte bt2;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct D3DWizMaterialSmall
    {
        public byte btType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        byte[] chName;       //[40]                                                                                 // Offset 0x01
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        byte[] chTextureFile;   //[40]                                                                            // Offset 0x29 // Usually there is only one texture, but in two cases there are tw textures
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        byte[] chTextureFile2;    //[40]                                                                        // Offset 0x51
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        byte[] chTextureFile3;    //[40]                                                                        // Offset 0x79
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        byte[] chTextureFile4;    //[40]                                                                        // Offset 0xA1
        public float rDiffuse, gDiffuse, bDiffuse;
        public float rAmbient, gAmbient, bAmbient;
        public float rEmissive, gEmissive, bEmissive;
        public float rSpecular, gSpecular, bSpecular;
        float f0;                                                                                                                                           // ?
        public float aDiffuse, aAmbient, aEmissive, aSpecular;
        byte bt;
        int n;                                                                                                                                            // Always 0, except for single case (it is 2)
        public float f2;                                                                                                                                           // != 0 only when ifl texture
        int nFlags;                                                                                                                   // 1,2,8

        public string Name
        {
            get { return System.Text.Encoding.UTF8.GetString(chName.TakeWhile(x => x != 0).ToArray()); }
        }
        public string TextureFile
        {
            get { return System.Text.Encoding.UTF8.GetString(chTextureFile.TakeWhile(x => x != 0).ToArray()); }
        }

    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct D3DWizMaterial
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        float[] f3; //4
        public byte btType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] chName;       //[40]                                                                                 // Offset 0x01
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] chTextureFile;   //[40]                                                                            // Offset 0x29 // Usually there is only one texture, but in two cases there are tw textures
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] chTextureFile2;    //[40]                                                                        // Offset 0x51
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] chTextureFile3;    //[40]                                                                        // Offset 0x79
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] chTextureFile4;    //[40]                                                                        // Offset 0xA1
        public float rDiffuse, gDiffuse, bDiffuse;
        public float rAmbient, gAmbient, bAmbient;
        public float rEmissive, gEmissive, bEmissive;
        public float rSpecular, gSpecular, bSpecular;
        float f0;                                                                                                                                           // ?
        public float aDiffuse, aAmbient, aEmissive, aSpecular;
        byte bt;
        int n;                                                                                                                                            // Always 0, except for single case (it is 2)
        public float f2;                                                                                                                                           // != 0 only when ifl texture
        int nFlags;
        public string Name
        {
            get { return System.Text.Encoding.UTF8.GetString(chName.TakeWhile(x => x != 0).ToArray()); }
        }
        public string TextureFile
        {
            get
            {
                return
                  System.Text.Encoding.Default.GetString(chTextureFile.TakeWhile(x => x != 0).ToArray());
            }
        }
    }


    public struct WZModel
    {
        public Mesh ItemMesh;
        public Material[] Materials;
    }

    public class ItemsFile
    {
        #region Fields

        const string rubyString = "Ruby";
        const string fallExeFilename = "FALL.EXE";
        const int defaultItemsOffset = 1776954;
        const int nameLength = 24;
        const int recordLength = 48;
        const int totalItems = 288;

        bool isOpen = false;
        int itemsOffset = defaultItemsOffset;

        string itemName;
        FileProxy file = new FileProxy();

        Mesh itemMesh = new Mesh();
        List<Material> itemMaterials = new List<Material>();
        Exception lastException = new Exception();

        #endregion

        #region Properties

        /// <summary>
        /// Gets static FALL.EXE filename.
        /// </summary>
        public static string Filename
        {
            get { return fallExeFilename; }
        }

        /// <summary>
        /// Gets path to FALL.EXE file.
        /// </summary>
        public string FilePath
        {
            get { return file.FilePath; }
        }

        /// <summary>
        /// Gets path to FALL.EXE file.
        /// </summary>
        public string Name
        {
            get { return (itemName != "" || itemName != null) ? itemName : "NoName"; }
        }

        /// <summary>
        /// Gets array of native item data.
        /// </summary>
        public Mesh ItemMesh
        {
            get { return itemMesh; }
        }

        /// <summary>
        /// Gets array of native item data.
        /// </summary>
        public Material[] ItemMaterials
        {
            get { return itemMaterials.ToArray(); }
        }

        /// <summary>
        /// Gets or sets the items offset.
        /// </summary>
        public int ItemsOffset
        {
            get { return itemsOffset; }
            set { itemsOffset = value; }
        }


        /// <summary>
        /// Gets file open flag.
        /// </summary>
        public bool IsOpen
        {
            get { return isOpen; }
        }

        /// <summary>
        /// Gets last exception.
        /// </summary>
        public Exception LastException
        {
            get { return lastException; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemsFile()
        {
        }

        /// <summary>
        /// Path constructor.
        /// </summary>
        /// <param name="fallExePath">Path to FALL.EXE.</param>
        public ItemsFile(string filePath, FileUsage usage = FileUsage.UseMemory, bool readOnly = true)
            : base()
        {
            Load(filePath, usage, readOnly);
        }

        /// <summary>
        /// Path constructor.
        /// </summary>
        /// <param name="fallExePath">Path to FALL.EXE.</param>
        public ItemsFile(byte[] data, string Name = "NoName", FileUsage usage = FileUsage.UseMemory, bool readOnly = true)
            : base()
        {
            Load(data, Name, usage, readOnly);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load a CFG file from disk.
        /// </summary>
        /// <param name="filePath">Absolute path to CLASS*.CFG file.</param>
        /// <param name="usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="readOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(byte[] data, string Name = "NoName", FileUsage usage = FileUsage.UseMemory, bool readOnly = true)
        {
            // Validate filename
            //string filename = Path.GetFileName(filePath);
            //if (!filename.EndsWith(".ITM", StringComparison.InvariantCultureIgnoreCase))
            //{
            //    return false;
            //}

            itemName = Name;

            // Load file
            if (!file.Load(data, "", usage, readOnly))
                return false;

            // Read file data
            BinaryReader reader = file.GetReader();
            ReadFile(reader);

            return true;
        }

        /// <summary>
        /// Load a CFG file from disk.
        /// </summary>
        /// <param name="filePath">Absolute path to CLASS*.CFG file.</param>
        /// <param name="usage">Specify if file will be accessed from disk, or loaded into RAM.</param>
        /// <param name="readOnly">File will be read-only if true, read-write if false.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string filePath, FileUsage usage = FileUsage.UseMemory, bool readOnly = true)
        {
            // Validate filename
            string filename = Path.GetFileName(filePath);
            if (!filename.EndsWith(".ITM", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            itemName = Path.GetFileNameWithoutExtension(filePath);

            // Load file
            if (!file.Load(filePath, usage, readOnly))
                return false;

            // Read file data
            BinaryReader reader = file.GetReader();
            ReadFile(reader);

            return true;
        }

        #endregion

        #region Private Methods


        #endregion

        #region File Readers

        void ReadFile(BinaryReader reader)
        {
            var header = file.Read<WZItemHeader>(reader);

            if (header.nType == 3)
            {
                reader.ReadBytes(44);
            }

            if (header.nFaces > 1000 || header.nVertices > 1000)
            {
                Debug.LogError("Error Reading ITM FILE!");
                return;
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            //SortedDictionary<int, Vector2> uvs = new SortedDictionary<int, Vector2>();
            Dictionary<int, List<int>> subMeshesTriangles = new Dictionary<int, List<int>>();




            if (header.nType > 1)
            {
                List<D3DWizFaceSmall> LstFaces = new List<D3DWizFaceSmall>();

                for (int i = 0; i < header.nVertices; i++)
                {
                    D3DWizVertexSmall vertex = file.Read<D3DWizVertexSmall>(reader);
                    vertices.Add(new Vector3(vertex.x, vertex.y, vertex.z));
                    uvs.Add(new Vector2(0,0));
                }

                for (int i = 0; i < header.nFaces; i++)
                {
                    D3DWizFaceSmall face = file.Read<D3DWizFaceSmall>(reader);
                    LstFaces.Add(face);
                }

                LstFaces = LstFaces.OrderBy(x => x.btMaterial).ToList();

                foreach (var face in LstFaces)
                {
                    if (!subMeshesTriangles.ContainsKey(face.btMaterial))
                    {
                        subMeshesTriangles.Add(face.btMaterial, new List<int>());
                    }

                    var shVertex1 = new Vector2(face.ftu1, -face.ftv1);
                    var shVertex2 = new Vector2(face.ftu2, -face.ftv2);
                    var shVertex3 = new Vector2(face.ftu3, -face.ftv3);

                    var InitVector2 = new Vector2(0, 0);

                    var idxVertex1 = 0;
                    var idxVertex2 = 0;
                    var idxVertex3 = 0;
                    if (uvs[face.shVertex1] == InitVector2)
                    {
                        uvs[face.shVertex1] = shVertex1;
                        idxVertex1 = face.shVertex1;
                    }else if (uvs[face.shVertex1] != shVertex1)
                    {
                        vertices.Add(vertices[face.shVertex1]);
                        uvs.Add(shVertex1);
                        idxVertex1 = vertices.Count-1;
                    } else
                    {
                        idxVertex1 = face.shVertex1;
                    }

                    if (uvs[face.shVertex2] == InitVector2)
                    {
                        uvs[face.shVertex2] = shVertex2;
                        idxVertex2 = face.shVertex2;
                    } else if (uvs[face.shVertex2] != shVertex2)
                    {
                        vertices.Add(vertices[face.shVertex2]);
                        uvs.Add(shVertex2);
                        idxVertex2 = vertices.Count-1;
                    } else
                    {
                        idxVertex2 = face.shVertex2;
                    }

                    if (uvs[face.shVertex3] == InitVector2)
                    {
                        uvs[face.shVertex3] = shVertex3;
                        idxVertex3 = face.shVertex3;
                    } else if (uvs[face.shVertex3] != shVertex3)
                    {
                        vertices.Add(vertices[face.shVertex3]);
                        uvs.Add(shVertex3);
                        idxVertex3 = vertices.Count-1;
                    } else
                    {
                        idxVertex3 = face.shVertex3;
                    }

                    subMeshesTriangles[face.btMaterial].AddRange(new List<int>() { idxVertex1, idxVertex2, idxVertex3 });

                }
            } else
            {

                List<D3DWizFace> LstFaces = new List<D3DWizFace>();

                for (int i = 0; i < header.nVertices; i++)
                {
                    D3DWizVertex vertex = file.Read<D3DWizVertex>(reader);
                    vertices.Add(new Vector3(vertex.x, vertex.y, vertex.z));
                    uvs.Add(new Vector2(0, 0));
                }

                for (int i = 0; i < header.nFaces; i++)
                {
                    D3DWizFace face = file.Read<D3DWizFace>(reader);
                    LstFaces.Add(face);
                }

                LstFaces = LstFaces.OrderBy(x => x.nMaterial).ToList();

                foreach (var face in LstFaces)
                {
                    if (!subMeshesTriangles.ContainsKey((int)face.nMaterial))
                    {
                        subMeshesTriangles.Add((int)face.nMaterial, new List<int>());
                    }

                    var nVertex1 = new Vector2(face.ftu1, -face.ftv1);
                    var nVertex2 = new Vector2(face.ftu2, -face.ftv2);
                    var nVertex3 = new Vector2(face.ftu3, -face.ftv3);

                    var InitVector2 = new Vector2(0, 0);

                    var idxVertex1 = 0;
                    var idxVertex2 = 0;
                    var idxVertex3 = 0;
                    if (uvs[(int)face.nVertex1] == InitVector2)
                    {
                        uvs[(int)face.nVertex1] = nVertex1;
                        idxVertex1 = (int)face.nVertex1;
                    } else if (uvs[(int)face.nVertex1] != nVertex1)
                    {
                        vertices.Add(vertices[(int)face.nVertex1]);
                        uvs.Add(nVertex1);
                        idxVertex1 = vertices.Count - 1;
                    } else
                    {
                        idxVertex1 = (int)face.nVertex1;
                    }

                    if (uvs[(int)face.nVertex2] == InitVector2)
                    {
                        uvs[(int)face.nVertex2] = nVertex2;
                        idxVertex2 = (int)face.nVertex2;
                    } else if (uvs[(int)face.nVertex2] != nVertex2)
                    {
                        vertices.Add(vertices[(int)face.nVertex2]);
                        uvs.Add(nVertex2);
                        idxVertex2 = vertices.Count - 1;
                    } else
                    {
                        idxVertex2 = (int)face.nVertex2;
                    }

                    if (uvs[(int)face.nVertex3] == InitVector2)
                    {
                        uvs[(int)face.nVertex3] = nVertex3;
                        idxVertex3 = (int)face.nVertex3;
                    } else if (uvs[(int)face.nVertex3] != nVertex3)
                    {
                        vertices.Add(vertices[(int)face.nVertex3]);
                        uvs.Add(nVertex3);
                        idxVertex3 = vertices.Count - 1;
                    } else
                    {
                        idxVertex3 = (int)face.nVertex3;
                    }

                    subMeshesTriangles[(int)face.nMaterial].AddRange(new List<int>() { idxVertex1, idxVertex2, idxVertex3 });

                }
            }

            var NumberOfMaterials = reader.ReadUInt16();

            D3DWizMaterialSmall mat1 = file.Read<D3DWizMaterialSmall>(reader);

            if (mat1.btType == 4)
            {

                var w8Unity = Wizardry8Unity.Instance;

                for (int i = 1; i < NumberOfMaterials; i++)
                {
                    var itemMaterial = new Material(Shader.Find("Standard"));

                    D3DWizMaterial mat = file.Read<D3DWizMaterial>(reader);

                    if (mat.TextureFile == "")
                    {
                        Debug.Log("Material TextureFilename Empty: ");
                        Debug.Log(mat.Name);
                        Debug.Log(mat.TextureFile);

                        itemMaterial.SetColor("_Color", new Color(mat.rDiffuse, mat.gDiffuse, mat.bDiffuse, mat.aDiffuse));
                        //  continue;
                    } else
                    {
                        var str = @"ITEMS3D\BITMAPS\" + mat.TextureFile.ToUpper();
                        var data = w8Unity.ContentReader.DataFileReader.GetFile(str);

                        if (mat.f2 != 1) //IFL FIle
                        {
                            Debug.Log("ifl Texture - Not Animating Yet!");

                            var firstTGAFile = "";

                            using (StreamReader file = new StreamReader(new MemoryStream(data)))
                            {
                                string line;
                                while ((line = file.ReadLine()) != null)
                                {

                                    firstTGAFile = line;

                                    break;
                                }

                                file.Close();
                            }

                            str = @"ITEMS3D\BITMAPS\" + firstTGAFile.ToUpper();
                            data = w8Unity.ContentReader.DataFileReader.GetFile(str);

                        }

                        var texture = TGALoader.LoadTGA(data);

                        itemMaterial.mainTexture = texture;
                    }

                    itemMaterial.SetFloat("_SmoothnessTextureChannel", 1);
                    itemMaterial.SetFloat("_Metallic", 0);
                    itemMaterial.SetFloat("_Glossiness", 1);

                    itemMaterial.SetOverrideTag("RenderType", "");
                    itemMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    itemMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    itemMaterial.SetInt("_ZWrite", 1);
                    itemMaterial.DisableKeyword("_ALPHATEST_ON");
                    itemMaterial.DisableKeyword("_ALPHABLEND_ON");
                    itemMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    itemMaterial.renderQueue = -1;

                    itemMaterial.SetColor("_EmissionColor", new Color(mat.rEmissive, mat.gEmissive, mat.bEmissive, mat.aEmissive));

                    itemMaterial.name = mat.Name;

                    this.itemMaterials.Add(itemMaterial);
                }


            } else
            {

                var w8Unity = Wizardry8Unity.Instance;

                for (int i = 1; i < NumberOfMaterials; i++)
                {
                    var itemMaterial = new Material(Shader.Find("Standard"));

                    D3DWizMaterialSmall mat = file.Read<D3DWizMaterialSmall>(reader);

                    if (mat.TextureFile == "")
                    {
                        Debug.Log("Material TextureFilename Empty: ");
                        Debug.Log(mat.Name);
                        Debug.Log(mat.TextureFile);

                        itemMaterial.SetColor("_Color", new Color(mat.rDiffuse, mat.gDiffuse, mat.bDiffuse, mat.aDiffuse));
                        //  continue;
                    } else
                    {
                        var str = @"ITEMS3D\BITMAPS\" + mat.TextureFile.ToUpper();
                        var data = w8Unity.ContentReader.DataFileReader.GetFile(str);
                        if (mat.f2 != 1)
                        {
                            Debug.LogError("ifl!");
                        }
                        var texture = TGALoader.LoadTGA(data);

                        itemMaterial.mainTexture = texture;
                    }


                    itemMaterial.SetColor("_EmissionColor", new Color(mat.rEmissive, mat.gEmissive, mat.bEmissive, mat.aEmissive));

                    itemMaterial.name = mat.Name;

                    this.itemMaterials.Add(itemMaterial);
                }
            }

            itemMesh = new Mesh();

            itemMesh.name = this.itemName;

            itemMesh.vertices = vertices.ToArray(); ;// vertices.ToArray();
            itemMesh.uv = uvs.ToArray(); ;// vertices.ToArray();

            itemMesh.subMeshCount = NumberOfMaterials - 1;

            foreach (var subm in subMeshesTriangles.Keys)
            {
                itemMesh.SetTriangles(subMeshesTriangles[subm], subm - 1);
            }



            // itemMesh.RecalculateNormals();

            UnityEngine.Debug.Log("finish");
        }

        #endregion
    }
}