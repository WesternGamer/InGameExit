using HarmonyLib;
using System.Reflection;
using VRage.Plugins;

namespace InGameExit
{
    public class Main : IPlugin
    {
        
        public void Startup()
        {

        }

        
        public void Dispose()
        {
            
        }

        
        public void Init(object gameInstance)
        { 
            Harmony harmony = new Harmony("SEPluginTemplate");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
                
        public void Update()
        {
            
        }
    }
}
