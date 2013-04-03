using System;

namespace Text
{
  // We don't need to do anything with our plug-in class yet.
  // It just needs to exist in the project
  public class TextPlugIn : Rhino.PlugIns.PlugIn
  {
    protected override Rhino.PlugIns.LoadReturnCode OnLoad(ref string errorMessage)
    {
    #if ON_OS_MAC
      MonoMac.ObjCRuntime.Runtime.RegisterAssembly(GetType().Assembly);
    #endif
      return base.OnLoad(ref errorMessage);
    }
  }
}

