using System;
using System.Collections.Generic;

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

        public InsertLineRequest(int lineIndex, string newLine, bool inheritWhitespace = true)
        {
            this.lineIndex = lineIndex;
            newLines = newLine.Split('\n');
            this.inheritWhitespace = inheritWhitespace;
        }

        public InsertLineRequest(int lineIndex, string[] newLines, bool inheritWhitespace = true)
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
                // idfk how to count whitespace ill do it later
            }
            fileLines.InsertRange(lineIndex + lineIndexOffset, newLines);
            lineIndexOffset += newLines.Length;
        }
    }

    public class ReplaceLineRequest : IFileModRequest
    {
        public int lineIndex;
        public string newLine;

        public ReplaceLineRequest(int lineIndex, string newLine)
        {
            this.lineIndex = lineIndex;
            this.newLine = newLine;
        }

        public int GetPriority()
        {
            return lineIndex;
        }

        public void ModifyFile(List<string> fileLines, ref int lineIndexOffset)
        {
            fileLines[lineIndex + lineIndexOffset] = newLine;
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

    public interface IFileModRequester
    {
        public IFileModRequest[] OnModifyFile();
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