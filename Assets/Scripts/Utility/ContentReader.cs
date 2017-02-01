
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using WizardryConnect;
using WizardryConnect.Utility;
using WizardryConnect.W8;
using System.Linq;

namespace WizardryConnect.Utility
{
    public class ContentReader
    {
        bool isReady = false;
        string w8Path;

        SLFFile dataFileReader;

        List<string> listItems3D;

        public bool IsReady
        {
            get { return isReady; }
        }

        public SLFFile DataFileReader
        {
            get { return dataFileReader; }
        }

        public string[] GetItem3DList
        {
            get { return listItems3D.ToArray(); }
        }

        #region Constructors

        public ContentReader(string w8Path)
        {
            this.w8Path = w8Path;
            SetupReaders();
        }

        #endregion


        public bool GetItem3D(string name, out Mesh Mesh, out Material[] Materials)
        {
            Mesh = new Mesh();
            Materials = new Material[0];
            if (!isReady)
                return false;

            byte[] data;
            if (!name.Contains("ITEMS3D"))
            {
                data = dataFileReader.GetFile(Path.Combine("ITEMS3D", name));
            }else
            {
                data = dataFileReader.GetFile(name);
            }
             

            if (data.Length == 0)
            {
                Wizardry8Unity.LogMessage(string.Format("Unknown Item '{0}'.", name), true);
            }
            var file = new ItemsFile(data, System.IO.Path.GetFileNameWithoutExtension(name));

            Mesh = file.ItemMesh;
            Materials = file.ItemMaterials;

            return true;
        }



        #region Private Methods

        /// <summary>
        /// Setup API file readers.
        /// </summary>
        private void SetupReaders()
        {
            if (dataFileReader == null)
                dataFileReader = new SLFFile(Path.Combine(w8Path, SLFFile.Filename));

            // Build map lookup dictionary
            if (listItems3D == null && dataFileReader != null)
                EnumerateItems3D();

            // Raise ready flag
            isReady = true;
        }

        /// <summary>
        /// Build dictionary of locations.
        /// </summary>
        private void EnumerateItems3D()
        {
            var listFiles=dataFileReader.GetListFiles();

            listItems3D = listFiles.Where(x => x.Contains("ITEMS3D") && !x.Contains("BITMAPS")).ToList();
        }

        #endregion
    }
}