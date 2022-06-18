﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.IO;
using RoR2EditorKit.Common;

namespace Packages.riskofthunder_ror2editorkit.Editor.Core
{
    [InitializeOnLoad]
    internal static class CopyXMLDoc
    {
        static CopyXMLDoc()
        {
            if(ShouldCopy())
            {
                Debug.Log($"Copying over the ROR2EK XML Doc");
                DoCopy();
            }
        }

        private static bool ShouldCopy()
        {
            var relativePath = AssetDatabase.GetAssetPath(Constants.XMLDoc);
            var fullPath = Path.GetFullPath(relativePath);
            var fileName = Path.GetFileName(fullPath);
            var pathToCheck = Path.Combine(Constants.FolderPaths.ScriptAssembliesFolder, fileName);
            return !File.Exists(pathToCheck);
        }

        private static void DoCopy()
        {
            var relativePath = AssetDatabase.GetAssetPath(Constants.XMLDoc);
            var sourcePath = Path.GetFullPath(relativePath);
            var fileName = Path.GetFileName(sourcePath);
            var destPath = Path.Combine(Constants.FolderPaths.ScriptAssembliesFolder, fileName);
            File.Copy(sourcePath, destPath, true);
        }
    }
}
