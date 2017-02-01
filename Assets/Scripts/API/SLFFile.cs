

#region Using Statements
using System;
using System.Text;
using System.IO;
using WizardryConnect.Utility;
using System.Runtime.InteropServices;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace WizardryConnect.W8
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SLFHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] sLibName;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] sPathToLibrary;
        public int iEntries;
        public int iUsed;
        public ushort iSort;
        public ushort iVersion;
        public int fContainsSubDirectories;
        public int iReserved;

        public string Name
        {
            get { return System.Text.Encoding.UTF8.GetString(sLibName.TakeWhile(x => x != 0).ToArray()); }
        }
        public string PathToLibrary
        {
            get { return System.Text.Encoding.UTF8.GetString(sPathToLibrary.TakeWhile(x => x != 0).ToArray()); }
        }

    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DIRENTRY
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] sFileName;
        public uint uiOffset;
        public uint uiLength;
        public byte ubState;
        public byte ubReserved;
        public SGP_FILETIME sFileTime;
        public uint usReserved2; //ushort
        public ushort something;

        public string Name
        {
            get { return System.Text.Encoding.UTF8.GetString(sFileName.TakeWhile(x => x != 0).ToArray()); }
        }
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SGP_FILETIME
    {
        public uint Lo;
        public uint Hi;
    };


    public class SLFFile
    {
        #region Class Variables


        public Dictionary<string, DIRENTRY> files = new Dictionary<string, DIRENTRY>();

        /// <summary>Abstracts PAK file to a managed disk or memory stream.</summary>
        private FileProxy managedFile = new FileProxy();

        #endregion

        #region Public Properties


        static public string Filename
        {
            get { return @"Data\DATA.SLF"; }
        }

        public byte[] GetFile(string path)
        {
            return ReadFile(path);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public SLFFile()
        {
        }

        /// <summary>
        /// Load constructor.
        /// </summary>
        /// <param name="filePath">Absolute path to PAK file.</param>
        public SLFFile(string filePath)
        {
            Load(filePath);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load PAK file.
        /// </summary>
        /// <param name="filePath">Absolute path to PAK file.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool Load(string filePath)
        {
            // Validate filename
            if (!filePath.EndsWith(".SLF", StringComparison.InvariantCultureIgnoreCase))
                return false;

            // Load file
            if (!managedFile.Load(filePath, FileUsage.UseMemory, true))
                return false;


            BinaryReader reader = managedFile.GetReader();

            var header = managedFile.Read<SLFHeader>(reader);
            var sizeEntry = (Marshal.SizeOf(typeof(DIRENTRY)));
            reader.BaseStream.Position = reader.BaseStream.Length - (header.iEntries * sizeEntry);

            for (int i = 0; i < header.iEntries; i++)
            {
                var entry = managedFile.Read<DIRENTRY>(reader);

                if (entry.ubState != 0)
                {
                    continue;
                }

                if (files.ContainsKey(entry.Name))
                {

                    Debug.LogError("File Name already Exists! : " + entry.Name);
                }

                files.Add(entry.Name, entry);
            }

            // Managed file is no longer needed
           // managedFile.Close();

            return true;
        }

        public List<string> GetListFiles()
        {
            return files.Keys.ToList();
        }

        #endregion

        #region Private Methods
        private byte[] ReadFile(string path)
        {
            if (!this.files.ContainsKey(path))
            {
                Debug.LogError("No path: " + path);
                return new byte[0];
            }

            var entry = this.files[path];

            BinaryReader reader = managedFile.GetReader((int)entry.uiOffset);

            return reader.ReadBytes((int)entry.uiLength);
        }
        #endregion
    }
}
