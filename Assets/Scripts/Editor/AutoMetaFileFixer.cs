using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Infrastructure.Managers.Editor
{
    [InitializeOnLoad]
    public static class AutoMetaFileFixer
    {
        static AutoMetaFileFixer()
        {
            //EditorApplication.delayCall += CheckAndFixCorruptedMetaFiles;
        }

        [MenuItem("Tools/Fix Corrupted Meta Files Now")]
        public static void FixCorruptedMetaFilesManually()
        {
            CheckAndFixCorruptedMetaFiles();
        }

        private static void CheckAndFixCorruptedMetaFiles()
        {
            try
            {
                string[] metaFiles = Directory.GetFiles(Application.dataPath, "*.meta", SearchOption.AllDirectories);
                int fixedCount = 0;

                foreach (string metaFile in metaFiles)
                {
                    if (IsMetaFileCorrupted(metaFile))
                    {
                        string relativePath = "Assets" + metaFile.Substring(Application.dataPath.Length);
                        
                        if (FixCorruptedMetaFile(metaFile, relativePath))
                        {
                            fixedCount++;
                        }
                    }
                }

                if (fixedCount > 0)
                {
                    Debug.Log($"Auto Meta File Fixer: Fixed {fixedCount} corrupted meta files. Refreshing Asset Database...");
                    AssetDatabase.Refresh();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Auto Meta File Fixer encountered an error: {e.Message}");
            }
        }

        private static bool IsMetaFileCorrupted(string filePath)
        {
            try
            {
                string content = File.ReadAllText(filePath);
                
                // Check for Git merge conflict markers
                if (content.Contains("<<<<<<<") || content.Contains(">>>>>>>") || content.Contains("======="))
                {
                    return true;
                }
                
                // Check for missing GUID
                if (!content.Contains("guid:"))
                {
                    return true;
                }
                
                // Check for malformed YAML structure
                if (!content.StartsWith("fileFormatVersion:"))
                {
                    return true;
                }
                
                // Check if content is too short (likely corrupted)
                if (content.Length < 50)
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

        private static bool FixCorruptedMetaFile(string fullPath, string relativePath)
        {
            try
            {
                string assetPath = relativePath.Substring(0, relativePath.Length - 5); // Remove .meta extension
                string fullAssetPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - 6), assetPath);
                
                // Check if the corresponding asset exists
                if (File.Exists(fullAssetPath) || Directory.Exists(fullAssetPath))
                {
                    // Delete the corrupted meta file and let Unity regenerate it
                    File.Delete(fullPath);
                    Debug.Log($"Deleted corrupted meta file: {relativePath}");
                    return true;
                }
                else
                {
                    // If the asset doesn't exist, delete the orphaned meta file
                    File.Delete(fullPath);
                    Debug.Log($"Deleted orphaned meta file: {relativePath}");
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to fix meta file {relativePath}: {e.Message}");
                return false;
            }
        }
    }
} 