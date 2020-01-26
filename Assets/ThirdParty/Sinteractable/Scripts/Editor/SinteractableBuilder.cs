
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
#if UNITY_EDITOR
using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class SinteractableBuilder : IPreprocessBuild
{
    private const string ClassTemplate = "using UnityEngine;\n" +
                                         "using Augmentix;\n" +
                                         "public class $CLASSNAME {\n" +
                                             "$VARIABLES\n" +
                                             "public void OnStart(){\n" +
                                                "$ON_START_STRING\n" +
                                             "}\n" +
                                             "\n" +
                                             "public void OnInteractionStart(ARHand hand){\n" +
                                                "$INTERARTION_START_STRING\n" +
                                             "}\n" +
                                             "\n" +
                                             "public void OnInteractionEnd(ARHand hand){\n" +
                                                "$INTERARTION_END_STRING\n" +
                                             "}\n" +
                                             "\n" +
                                             "public void OnInteractionKeep(ARHand hand){\n" +
                                                "$INTERARTION_KEEP_STRING\n" +
                                             "}\n" +
                                         "}\n";
    
    
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        var assembly = Assembly.GetAssembly(typeof(SinteractableBuilder));
        var classlist = new List<string>();
        foreach (var sinteractable in UnityEngine.Object.FindObjectsOfType<Sinteractable>())
        {
            var classStrings = SinteractableToString(sinteractable);
            var classname = classStrings.Item1;
            var classtext = classStrings.Item2;
            
            
        }
        
        
    }
    
    public class MyBuildPostprocessor {
        [PostProcessBuild(1)]
        public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
            Debug.Log( pathToBuiltProject );
        }
    }

    private (string,string) SinteractableToString(Sinteractable sinteractable)
    {
        var classname = SinToHashName(sinteractable);
        var s = ClassTemplate.Replace("$CLASSNAME",classname );
        s = s.Replace("$VARIABLES", sinteractable.Variables.Select(variable =>
        {
            if (variable.EndsWith(";"))
                return variable;
            return variable + ";";
        }).Aggregate((s1, s2) => { return s1 + "\n" + s2; }));

        s = s.Replace("$ON_START_STRING", sinteractable.OnStart);
        s = s.Replace("$INTERARTION_START_STRING", sinteractable.OnInteractionStartString);
        s = s.Replace("$INTERARTION_END_STRING", sinteractable.OnInteractionEndString);
        s = s.Replace("$INTERARTION_KEEP_STRING", sinteractable.OnInteractionKeepString);
        
        Debug.Log("SINTERACTABLE: "+s);
        return (classname,s);
    }

    private string SinToHashName(Sinteractable sinteractable)
    {
        var nameBytes = System.Text.Encoding.ASCII.GetBytes(sinteractable.GetHashCode().ToString());
        var base64 = Convert.ToBase64String(nameBytes);
        return "S" + base64.Substring(0, base64.Length - 1);
    }
}
#endif
