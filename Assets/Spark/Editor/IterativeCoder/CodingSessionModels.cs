using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace LeastSquares.Spark
{

    [Serializable]
    public struct CodeVersion
    {
        public string code;
        public bool isPartial;
    }

    [Serializable]
    public class CodingSession : Conversation
    {
        [SerializeField]
        public List<string> CodeVersions = new ();
        [SerializeField] 
        public List<CodeVersion> Versions = new ();
        [SerializeField]
        public string ScriptPath;

        public override void Migrate()
        {
            if (CodeVersions.Count == 0) return;
            var transformed = CodeVersions.Select(s => new CodeVersion
            {
                code = s,
                isPartial = false
            });
            Versions = transformed.Concat(Versions).ToList();
            CodeVersions.Clear();
        }
        
        public bool IsLastCodeVersionPartial => Versions.Last().isPartial;

        public string LastCodeVersion => Versions.Last().code;

        public void ReloadCodeFromSource()
        {
            var version = Versions[Versions.Count - 1];
            version.code = File.ReadAllText(ScriptPath);
            Versions[Versions.Count - 1] = version;
        }

        public void AddOrReplaceCodeVersion(string code, bool isFinal = false)
        {
            if (Versions.Count > 0 && Versions[Versions.Count - 1].isPartial)
                Versions.RemoveAt(Versions.Count - 1);
            
            Versions.Add(new CodeVersion
            {
                code = code,
                isPartial = !isFinal
            });
        }

        public override void CancelLast()
        {
            base.CancelLast();
            if (Versions.Count > 0 && Versions[Versions.Count - 1].isPartial)
                Versions.RemoveAt(Versions.Count - 1);
        }
        
        public void Pop()
        {
            Versions.RemoveAt(Versions.Count-1);
            Messages.RemoveAt(Messages.Count-1);
            Messages.RemoveAt(Messages.Count-1);
        }
    }
}