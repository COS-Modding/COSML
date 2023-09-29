using COSML.Modding;
using MonoMod;

namespace COSML.Patches
{
    [MonoModPatch("global::GameController")]
    public class GameController : global::GameController
    {
        public void OnApplicationQuit()
        {
            ModHooks.OnApplicationQuit();
        }
    }
}
