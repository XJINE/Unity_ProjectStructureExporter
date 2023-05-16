using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ProjectStructureExporter
{
    public class ProjectStructureExporterWindow : EditorWindow
    {
        #region Field

        public string NestCharacter   = "-";
        public bool   IgnoreMetaFiles = true;

        #endregion Field

        #region Method

        [MenuItem("Custom/ProjectStructureExporter")]
        static void Init()
        {
            GetWindow<ProjectStructureExporterWindow>("ProjectStructureExporter");
        }

        protected void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Nest Character");
            NestCharacter = EditorGUILayout.TextField(NestCharacter);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Ignore .meta");
            IgnoreMetaFiles = EditorGUILayout.Toggle(IgnoreMetaFiles);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Export"))
            {
                Export();
            }
        }

        private void Export()
        {
            var files  = new FileStructure(Application.dataPath);
            var nest   = "";
            var result = "";

            void PrintFile(FileStructure fileStructure, string nest)
            {
                if (IgnoreMetaFiles && Path.GetExtension(fileStructure.Path) == ".meta")
                {
                    return;
                }
        
                result += nest + Path.GetFileName(fileStructure.Path) + "\n";
        
                if (fileStructure.IsDirectory)
                {
                    foreach (var childFile in fileStructure.Files)
                    {
                        PrintFile(childFile, nest + NestCharacter);
                    }
                }
            }

            PrintFile(files, nest);
            result = result.Trim();

            // CAUTION:
            // This will be null in "AssetDatabase.LoadAssetAtPath".
            // var exportFilePath = Path.Combine(Application.dataPath, "FileStructure.txt");

            var exportFilePath = "Assets/FileStructure.txt";
                exportFilePath = AssetCreationHelper.CorrectAssetNameToAvoidOverwrite(exportFilePath);

            File.WriteAllText(exportFilePath, result);

            AssetDatabase.Refresh();

            AssetCreationHelper.StartToRenameAsset(exportFilePath);
        }

        #endregion Method

        private class FileStructure
        {
            #region Property

            public string                            Path        { get; private set; }
            public bool                              IsDirectory { get; private set; }
            public ReadOnlyCollection<FileStructure> Files       { get; private set; }

            #endregion Property
        
            #region Constructor

            public FileStructure(string path)
            {
                Path        = path;
                IsDirectory = Directory.Exists(path);

                if (IsDirectory)
                {
                    var filePaths = Directory.GetDirectories(path).Union(Directory.GetFiles(path)).ToArray();
                    var files = new List<FileStructure>(filePaths.Length);
                        Files = new ReadOnlyCollection<FileStructure>(files);

                    files.AddRange(filePaths.Select(filePath => new FileStructure(filePath)));
                }
            }

            #endregion
        }
    }
}