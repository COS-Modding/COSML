using COSML.Log;
using COSML.Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace COSML;

internal class Preloader : MonoBehaviour
{
    public IEnumerator Preload
    (
        Dictionary<string, List<(COSML.ModInstance, List<string>)>> toPreload,
        Dictionary<COSML.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
        Dictionary<string, List<Func<IEnumerator>>> sceneHooks
    )
    {
        yield return DoPreload(toPreload, preloadedObjects, sceneHooks);

        yield return CleanUpPreloading();

        yield return null;
    }

    /// <summary>
    ///     This is the actual preloading process.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoPreload
    (
        Dictionary<string, List<(COSML.ModInstance Mod, List<string> Preloads)>> toPreload,
        IDictionary<COSML.ModInstance, Dictionary<string, Dictionary<string, GameObject>>> preloadedObjects,
        Dictionary<string, List<Func<IEnumerator>>> sceneHooks
    )
    {
        List<string> sceneNames = toPreload.Keys.Union(sceneHooks.Keys).ToList();
        Dictionary<string, int> scenePriority = new();
        Dictionary<string, (AsyncOperation load, AsyncOperation unload)> sceneAsyncOperationHolder = new();

        foreach (string sceneName in sceneNames)
        {
            int priority = 0;

            if (toPreload.TryGetValue(sceneName, out var requests))
                priority += requests.Select(x => x.Preloads.Count).Sum();

            scenePriority[sceneName] = priority;
            sceneAsyncOperationHolder[sceneName] = (null, null);
        }

        Dictionary<string, GameObject> GetModScenePreloadedObjects(COSML.ModInstance mod, string sceneName)
        {
            if (!preloadedObjects.TryGetValue
            (
                mod,
                out Dictionary<string, Dictionary<string, GameObject>> modPreloadedObjects
            ))
            {
                preloadedObjects[mod] = modPreloadedObjects = new Dictionary<string, Dictionary<string, GameObject>>();
            }

            // ReSharper disable once InvertIf
            if (!modPreloadedObjects.TryGetValue
            (
                sceneName,
                out Dictionary<string, GameObject> modScenePreloadedObjects
            ))
            {
                modPreloadedObjects[sceneName] = modScenePreloadedObjects = new Dictionary<string, GameObject>();
            }

            return modScenePreloadedObjects;
        }

        var preloadOperationQueue = new List<AsyncOperation>(5);

        IEnumerator GetPreloadObjectsOperation(string sceneName)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);

            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (var go in rootObjects) go.SetActive(false);

            if (sceneHooks.TryGetValue(scene.name, out List<Func<IEnumerator>> hooks))
            {
                // ToArray to force a strict select, that way we start them all simultaneously
                foreach (IEnumerator hook in hooks.Select(x => x()).ToArray()) yield return hook;
            }

            if (!toPreload.TryGetValue(sceneName, out var sceneObjects)) yield break;

            // Fetch object names to preload
            foreach ((COSML.ModInstance mod, List<string> objNames) in sceneObjects)
            {
                Logging.API.Debug($"Fetching objects for mod \"{mod.Mod.GetName()}\"");

                Dictionary<string, GameObject> scenePreloads = GetModScenePreloadedObjects(mod, sceneName);

                foreach (string objName in objNames)
                {
                    Logging.API.Debug($"Fetching object \"{objName}\"");

                    GameObject obj;

                    try
                    {
                        obj = UnityUtils.GetGameObjectFromArray(rootObjects, objName);
                    }
                    catch (ArgumentException)
                    {
                        Logging.API.Warn($"Invalid GameObject name {objName}");
                        continue;
                    }

                    if (obj == null)
                    {
                        Logging.API.Warn($"Could not find object \"{objName}\" in scene \"{sceneName}\"," + $" requested by mod `{mod.Mod.GetName()}`");
                        continue;
                    }

                    // Create inactive duplicate of requested object
                    obj = Instantiate(obj);
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);

                    // Set object to be passed to mod
                    scenePreloads[objName] = obj;
                }
            }
        }

        void CleanupPreloadOperation(string sceneName)
        {
            Logging.API.Debug($"Unloading scene \"{sceneName}\"");

            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);

            sceneAsyncOperationHolder[sceneName] = (sceneAsyncOperationHolder[sceneName].load, unloadOp);

            unloadOp.completed += _ => preloadOperationQueue.Remove(unloadOp);

            preloadOperationQueue.Add(unloadOp);
        }

        void StartPreloadOperation(string sceneName)
        {
            IEnumerator DoLoad(AsyncOperation load)
            {
                yield return load;

                preloadOperationQueue.Remove(load);
                yield return GetPreloadObjectsOperation(sceneName);
                CleanupPreloadOperation(sceneName);
            }

            Logging.API.Debug($"Loading scene \"{sceneName}\"");

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            StartCoroutine(DoLoad(loadOp));

            sceneAsyncOperationHolder[sceneName] = (loadOp, null);

            loadOp.priority = scenePriority[sceneName];

            preloadOperationQueue.Add(loadOp);
        }

        int i = 0;

        float sceneProgressAverage = 0;

        while (sceneProgressAverage < 1.0f)
        {
            while (
                preloadOperationQueue.Count < ModHooks.GlobalSettings.PreloadBatchSize &&
                i < sceneNames.Count &&
                sceneProgressAverage < 1.0f
            )
            {
                StartPreloadOperation(sceneNames[i++]);
            }

            yield return null;

            sceneProgressAverage = sceneAsyncOperationHolder
                                   .Values
                                   .Select(x => (x.load?.progress ?? 0) * 0.5f + (x.unload?.progress ?? 0) * 0.5f)
                                   .Average();
        }
    }

    /// <summary>
    ///     Clean up everything from preloading.
    /// </summary>
    /// <returns></returns>
    private IEnumerator CleanUpPreloading()
    {
        // Reload the main menu to fix the music/shaders
        Logging.API.Debug("Preload done, returning to main menu");

        COSML.LoadState |= COSML.ModLoadState.Preloaded;

        yield return SceneManager.LoadSceneAsync(Constants.SPLASH_SCREEN_SCENE_NAME);

        Logging.API.Debug(SceneManager.GetActiveScene().name);

        while (SceneManager.GetActiveScene().name != Constants.MAIN_MENU_SCENE_NAME)
        {
            yield return new WaitForEndOfFrame();
        }
    }
}
