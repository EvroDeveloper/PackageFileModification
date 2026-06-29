#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

namespace EvroDev.FileModLib
{
    [InitializeOnLoad]
    public static class FileModifier
    {
        static FileModifier()
        {
            foreach(Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if(assembly.FullName.StartsWith("Unity")) continue; // you arent going to find file modifiers in unity assmebly silly

                RunFileModifiers(FindFileModifiersInAssembly(assembly));
            }
        }

        static Dictionary<string, List<IFileModRequest>> FindFileModifiersInAssembly(Assembly assembly)
        {
            Dictionary<string, List<IFileModRequest>> output = new();
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<FileModifierAttribute>() != null)
                {
                    FileModifierAttribute attributeData = type.GetCustomAttribute<FileModifierAttribute>();
                    string filePath = attributeData.FilePath;

                    if(!File.Exists(filePath))
                    {
                        Debug.LogError("[File Modifier]: Could not find file to modify at path: " + filePath);
                        continue;
                    }
                    

                    if (typeof(IFileModRequester).IsAssignableFrom(type))
                    {
                        IFileModRequester modRequester = Activator.CreateInstance(type) as IFileModRequester;

                        if(!output.ContainsKey(filePath)) output.Add(filePath, new List<IFileModRequest>());

                        IFileModRequest[] fileModRequestsFromType = modRequester.OnModifyFile();
                        foreach(IFileModRequest request in fileModRequestsFromType)
                        {
                            SortAdd(output[filePath], request);
                        }
                    }
                }
            }
            return output;
        }

        static void SortAdd(List<IFileModRequest> list, IFileModRequest element)
        {
            int elementPriority = element.GetPriority();
            for(int i = 0; i < list.Count; i++)
            {
                if(list[i].GetPriority() > elementPriority)
                {
                    list.Insert(i, element);
                    return;
                }
            }
            list.Add(element);
        }

        static void RunFileModifiers(Dictionary<string, List<IFileModRequest>> fileToModifierList)
        {
            foreach(var fileModifiers in fileToModifierList)
            {
                string filePath = fileModifiers.Key;
                if(!filePath.StartsWith("Packages") && !filePath.StartsWith("Assets")) continue; // Safety measure, only allow editing of unity stuff
                if(!File.Exists(filePath)) continue;
                
                List<string> fileLines = new List<string>(File.ReadAllLines(filePath));
                string currentFileModList = fileLines[0];

                if(currentFileModList.StartsWith("/*filemods"))
                {
                    // Debugging stop
                    return;
                    fileLines.RemoveAt(0);
                }

                int lineModOffset = -1; // Text editors are off by one here, if something requests addition at line 1, start by inserting it at line 0

                foreach(IFileModRequest request in fileModifiers.Value)
                {
                    // Check if request has already been satisfied in header. If so, skip it
                    request.ModifyFile(fileLines, ref lineModOffset);
                    // add request identifier to list
                }

                // Add all request identifiers that were ran, and insert back into the header

                fileLines.Insert(0, "/*filemods:{}*/"); // Debugging stopper
                File.WriteAllLines(filePath, fileLines.ToArray());
            }
        }
    }
}
#endif