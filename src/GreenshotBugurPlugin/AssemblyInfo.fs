namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("GreenshotBugurPlugin")>]
[<assembly: AssemblyProductAttribute("GreenshotBugurPlugin")>]
[<assembly: AssemblyDescriptionAttribute("A plugin for Greenshot that uploads images to Bugur.")>]
[<assembly: AssemblyVersionAttribute("1.1")>]
[<assembly: AssemblyFileVersionAttribute("1.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.1"
