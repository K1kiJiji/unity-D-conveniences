using System;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Project.Scripts.Conveniences
{
    [Serializable]
    public class SceneLoader : ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector] public string scenePath;
        [SerializeField, HideInInspector] private string sceneGuid;


#if UNITY_EDITOR

        [SerializeField] private SceneAsset sceneAsset;
#endif


        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR

            var path = AssetDatabase.GetAssetPath(sceneAsset);
            scenePath = path;
            sceneGuid = AssetDatabase.AssetPathToGUID(path);
#endif
        }


        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            if (sceneGuid != null && sceneAsset == null)
            {
                var path = AssetDatabase.GUIDToAssetPath(sceneGuid);
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                sceneAsset = asset;
                scenePath  = path;
            }
#endif
        }


        public int GetIndex => string.IsNullOrEmpty(scenePath) ? -1 : SceneUtility.GetBuildIndexByScenePath(scenePath);

        public bool TryGetIndex(out int index)
        {
            index = GetIndex;
            return index >= 0;
        }

        public void Load(LoadSceneMode sceneMode = LoadSceneMode.Single)
        {
            if (!TryGetIndex(out var loadIndex))
            {
                return;
            }

            SceneManager.LoadScene(loadIndex, sceneMode);
        }

        public AsyncOperation LoadAsync(LoadSceneMode sceneMode = LoadSceneMode.Single, bool activateOnLoad = true)
        {
            if (!TryGetIndex(out var loadIndex))
            {
                return null;
            }

            var asyncOperation = SceneManager.LoadSceneAsync(loadIndex, sceneMode);
            if (asyncOperation != null)
            {
                asyncOperation.allowSceneActivation = activateOnLoad;
            }

            return asyncOperation;
        }
    }
}