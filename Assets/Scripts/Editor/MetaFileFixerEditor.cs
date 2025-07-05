using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Managers.Editor
{
    public class MetaFileFixerEditor : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> corruptedMetaFiles = new List<string>();
        private List<string> fixedFiles = new List<string>();
        private bool isScanning = false;
        private bool autoFix = false;

        [MenuItem("Tools/Meta File Fixer")]
        public static void ShowWindow()
        {
            GetWindow<MetaFileFixerEditor>("Meta File Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Meta File Fixer", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool scans for corrupted .meta files and fixes them by either cleaning up merge conflicts or regenerating them.",
                MessageType.Info);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scan for Corrupted Meta Files", GUILayout.Height(30)))
            {
                ScanForCorruptedMetaFiles();
            }

            autoFix = EditorGUILayout.Toggle("Auto Fix", autoFix, GUILayout.Width(80));
            EditorGUILayout.EndHorizontal();

            if (isScanning)
            {
                EditorGUILayout.HelpBox("Scanning...", MessageType.Info);
                return;
            }

            GUILayout.Space(10);

            if (corruptedMetaFiles.Count > 0)
            {
                GUILayout.Label($"Found {corruptedMetaFiles.Count} corrupted meta files:", EditorStyles.boldLabel);
                
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
                foreach (string file in corruptedMetaFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(file, EditorStyles.wordWrappedLabel);
                    if (GUILayout.Button("Fix", GUILayout.Width(50)))
                    {
                        FixMetaFile(file);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);
                if (GUILayout.Button("Fix All Corrupted Meta Files", GUILayout.Height(30)))
                {
                    FixAllCorruptedMetaFiles();
                }
            }
            else if (corruptedMetaFiles.Count == 0 && !isScanning)
            {
                EditorGUILayout.HelpBox("No corrupted meta files found.", MessageType.Info);
            }

            if (fixedFiles.Count > 0)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Fixed {fixedFiles.Count} files:", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100));
                foreach (string file in fixedFiles)
                {
                    EditorGUILayout.LabelField(file, EditorStyles.wordWrappedLabel);
                }
                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Clear Fixed Files List"))
                {
                    fixedFiles.Clear();
                }
            }
        }

        private void ScanForCorruptedMetaFiles()
        {
            isScanning = true;
            corruptedMetaFiles.Clear();
            
            try
            {
                string[] metaFiles = Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories);
                
                foreach (string metaFile in metaFiles)
                {
                    if (IsMetaFileCorrupted(metaFile))
                    {
                        string relativePath = "Assets" + metaFile.Substring(Application.dataPath.Length);
                        corruptedMetaFiles.Add(relativePath);
                        
                        if (autoFix)
                        {
                            FixMetaFile(relativePath);
                        }
                    }
                }
                
                Debug.Log($"Scan completed. Found {corruptedMetaFiles.Count} corrupted meta files.");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during scan: {e.Message}");
            }
            finally
            {
                isScanning = false;
            }
        }

        private bool IsMetaFileCorrupted(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                
                // Check for Git merge conflict markers
                if (content.Contains("<<<<<<<") || content.Contains(">>>>>>>") || content.Contains("======="))
                {
                    return true;
                }
                
                // Check for missing or invalid GUID
                if (!content.Contains("guid:") || !HasValidGuid(content))
                {
                    return true;
                }
                
                // Check for malformed YAML structure
                if (!content.StartsWith("fileFormatVersion:"))
                {
                    return true;
                }
                
                return false;
            }
            catch
            {
                return true; // If we can't read the file, consider it corrupted
            }
        }

        private bool HasValidGuid(string content)
        {
            Regex guidRegex = new Regex(@"guid:\s*([a-fA-F0-9]{32})");
            Match match = guidRegex.Match(content);
            return match.Success && match.Groups[1].Value.Length == 32;
        }

        private void FixMetaFile(string relativePath)
        {
            try
            {
                string fullPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), relativePath);
                string assetPath = relativePath.Substring(0, relativePath.Length - 5); // Remove .meta extension
                
                // Check if the corresponding asset exists
                if (File.Exists(Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), assetPath)) ||
                    Directory.Exists(Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), assetPath)))
                {
                    // Delete the corrupted meta file and let Unity regenerate it
                    File.Delete(fullPath);
                    fixedFiles.Add($"Deleted and regenerated: {relativePath}");
                    Debug.Log($"Deleted corrupted meta file: {relativePath}");
                }
                else
                {
                    // If the asset doesn't exist, just delete the orphaned meta file
                    File.Delete(fullPath);
                    fixedFiles.Add($"Deleted orphaned: {relativePath}");
                    Debug.Log($"Deleted orphaned meta file: {relativePath}");
                }
                
                corruptedMetaFiles.Remove(relativePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to fix meta file {relativePath}: {e.Message}");
            }
        }

        private void FixAllCorruptedMetaFiles()
        {
            List<string> filesToFix = new List<string>(corruptedMetaFiles);
            
            foreach (string file in filesToFix)
            {
                FixMetaFile(file);
            }
            
            // Refresh the asset database to regenerate meta files
            AssetDatabase.Refresh();
            
            Debug.Log($"Fixed {filesToFix.Count} corrupted meta files. Asset database refreshed.");
        }
    }
} 