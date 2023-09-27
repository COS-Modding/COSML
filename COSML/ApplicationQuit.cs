using COSML.Modding;
using UnityEngine;

namespace COSML
{
    internal class ApplicationQuit : MonoBehaviour
    {
        public void OnApplicationQuit()
        {
            ModHooks.OnApplicationQuit();
        }
    }
}
