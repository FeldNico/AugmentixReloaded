using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

public class Sinteractable : MonoBehaviour
{
    public UnityAction<ARHand> OnInteractionStart;
    public UnityAction<ARHand> OnInteractionEnd;
    public UnityAction<ARHand> OnInteractionKeep;
    
    /*
    public void Awake()
    {
        OnPreprocessBuild();
    }
    
    public void Start()
    {
        var type = HashNameToType(GetHashCode().ToString());
        var obj = gameObject.AddComponent(type);
        type.GetMethod("OnStart").Invoke(obj,null);
    }
*/
    public void Update()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        Debug.Log(vertical+" : "+horizontal);
        vertical = Input.GetAxis("Mouse X");
        horizontal = Input.GetAxis("Mouse Y");
        Debug.Log(vertical+" : "+horizontal);
    }

    public string[] Variables;
    
    [TextArea,Header("OnStart()")]
    public string OnStart;

    [TextArea,Header("OnInteractionStart(ARHand hand)")]
    public string OnInteractionStartString;

    [TextArea,Header("OnInteractionEnd(ARHand hand)")]
    public string OnInteractionEndString;

    [TextArea,Header("OnInteractionKeep(ARHand hand)")]
    public string OnInteractionKeepString;

    
    private const string ClassTemplate = "using UnityEngine;\n" +
                                         "using Augmentix;\n" +
                                         "public class $CLASSNAME : MonoBehaviour {\n" +
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

    
    /*
    public void OnPreprocessBuild()
    {
        CompilerParameters parameters = new CompilerParameters();
        // Add ALL of the assembly references
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                if (assembly.Location != null && assembly.Location != "")
                    parameters.ReferencedAssemblies.Add(assembly.Location);
            }
            catch
            {
                
            }
        }
        
        parameters.GenerateExecutable = false;
        parameters.OutputAssembly = "Sinteractable.dll";
        parameters.TreatWarningsAsErrors = false;

        var classlist = new List<string>();
        foreach (var sinteractable in UnityEngine.Object.FindObjectsOfType<Sinteractable>())
        {
            classlist.Add(SinteractableToString(sinteractable));
        }
        var results = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(parameters, classlist.ToArray());
        _assembly = results.CompiledAssembly;
    }

    private string SinteractableToString(Sinteractable sinteractable)
    {
        var s = ClassTemplate.Replace("$CLASSNAME", SinToHashName(sinteractable));
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
        
        return s;
    }

    private string SinToHashName(Sinteractable sinteractable)
    {
        var nameBytes = System.Text.Encoding.ASCII.GetBytes(sinteractable.GetHashCode().ToString());
        var base64 = Convert.ToBase64String(nameBytes);
        return "S" + base64.Substring(0, base64.Length-1);
    }

    private Type HashNameToType(string s)
    {
        var nameBytes = System.Text.Encoding.ASCII.GetBytes(s);
        var base64 = Convert.ToBase64String(nameBytes);
        var classname = "S" + base64.Substring(0, base64.Length-1);
        foreach (var type in _assembly.GetTypes())
        {
            if (type.Name == classname)
            {
                return type;
            }
        }

        return null;
    }
    */
}
