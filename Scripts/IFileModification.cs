using System;
using System.Collections.Generic;
using System.Linq;

namespace EvroDev.FileModLib
{
    public interface IFileModRequest
    {
        public void ModifyFile(List<string> fileLines, ref int lineIndexOffset);
        public int GetPriority();
    }

    public class ReplaceFileRequest : IFileModRequest
    {
        public string[] newFileLines;

        public ReplaceFileRequest(string newFile)
        {
            this.newFileLines = newFile.Split('\n');
        }

        public ReplaceFileRequest(string[] newFileLines)
        {
            this.newFileLines = newFileLines;
        }
        
        public void ModifyFile(List<string> fileLines, ref int lineIndexOffset)
        {
            fileLines.Clear();
            fileLines.AddRange(newFileLines);
        }

        public int GetPriority()
        {
            return 0;
        }
    }

    public class InsertLineRequest : IFileModRequest
    {
        public int lineIndex;
        public string[] newLines;
        public bool inheritWhitespace;

        public InsertLineRequest(int lineIndex, string newLine, bool inheritWhitespace = false)
        {
            this.lineIndex = lineIndex;
            newLines = newLine.Split('\n');
            this.inheritWhitespace = inheritWhitespace;
        }

        public InsertLineRequest(int lineIndex, string[] newLines, bool inheritWhitespace = false)
        {
            this.lineIndex = lineIndex;
            this.newLines = newLines;
            this.inheritWhitespace = inheritWhitespace;
        }

        public int GetPriority()
        {
            return lineIndex;
        }

        public void ModifyFile(List<string> fileLines, ref int lineIndexOffset)
        {
            if (inheritWhitespace)
            {
                string whitespace = GetWhitespace(fileLines[lineIndex + lineIndexOffset]);
                IEnumerable<string> appendedWhitespace = newLines.Select((line) => whitespace + line);
                fileLines.InsertRange(lineIndex + lineIndexOffset, appendedWhitespace);
            }
            else
            {
                fileLines.InsertRange(lineIndex + lineIndexOffset, newLines);
            }
            lineIndexOffset += newLines.Length;
        }

        private static string GetWhitespace(string line)
        {
            string outputWhitespace = "";
            for(int i = 0; i < line.Length; i++)
            {
                if(line[i] == ' ' || line[i] == '\t') outputWhitespace += line[i];
                else break;
            }
            return outputWhitespace;
        }
    }

    public class ReplaceLineRequest : IFileModRequest
    {
        public int lineIndex;
        public string newLine;
        public bool inheritWhitespace;

        public ReplaceLineRequest(int lineIndex, string newLine, bool inheritWhitespace = false)
        {
            this.lineIndex = lineIndex;
            this.newLine = newLine;
            this.inheritWhitespace = inheritWhitespace;
        }

        public int GetPriority()
        {
            return lineIndex;
        }

        public void ModifyFile(List<string> fileLines, ref int lineIndexOffset)
        {
            if(inheritWhitespace)
                fileLines[lineIndex + lineIndexOffset] = GetWhitespace(fileLines[lineIndex + lineIndexOffset]) + newLine;
            else
                fileLines[lineIndex + lineIndexOffset] = newLine;
        }
        
        private static string GetWhitespace(string line)
        {
            string outputWhitespace = "";
            for(int i = 0; i < line.Length; i++)
            {
                if(line[i] == ' ' || line[i] == '\t') outputWhitespace += line[i];
                else break;
            }
            return outputWhitespace;
        }
    }

    public class DeleteLineRequest : IFileModRequest
    {
        public int lineIndex;

        public DeleteLineRequest(int lineIndex)
        {
            this.lineIndex = lineIndex;
        }

        public int GetPriority()
        {
            return lineIndex;
        }

        public void ModifyFile(List<string> fileLines, ref int lineIndexOffset)
        {
            fileLines.RemoveAt(lineIndex + lineIndexOffset);
            lineIndexOffset -= 1;
        }
    }

    public abstract class FileModRequester
    {
        internal List<IFileModRequest> fileModRequests = new();
        public abstract void OnModifyFile();

        public void Request(IFileModRequest request)
        {
            fileModRequests.Add(request);
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class FileModifierAttribute : Attribute
    {
        public FileModifierAttribute(string targetFilePath)
        {
            FilePath = targetFilePath;
        }

        public string FilePath { get; private set; }
    }
}