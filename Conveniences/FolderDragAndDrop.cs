using System;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
#endif


namespace Project.Scripts.Conveniences
{
    [Serializable]
    public sealed class FolderDragAndDrop : ISerializationCallbackReceiver
    {
        [Flags]
        public enum AssetFilter
        {
            None          = 0,
            Texture2D     = 1 << 0,
            Sprite        = 1 << 1,
            AudioClip     = 1 << 2,
            VideoClip     = 1 << 3,
            TextAsset     = 1 << 4,
            Material      = 1 << 5,
            Model         = 1 << 6,
            AnimationClip = 1 << 7,
            Shader        = 1 << 8,
            Prefab        = 1 << 9,
            ScriptableObj = 1 << 10,
            Mesh          = 1 << 11,
            RenderTexture = 1 << 12,
            Cubemap       = 1 << 13,
            Texture3D     = 1 << 14,
            Font          = 1 << 15,
        }


#if UNITY_EDITOR

        [Header("Editor")]
        [SerializeField] private DefaultAsset folder;
        [SerializeField] private bool includeSubfolders;
        [SerializeField] private AssetFilter searchFilter = AssetFilter.None;
        [SerializeField] private bool autoRefresh;
        [SerializeField] private bool sortByName;
#endif


        [Header("Runtime (Serialized)")]
        [SerializeField] private UnityEngine.Object[] assets = Array.Empty<UnityEngine.Object>();

        public UnityEngine.Object[] Assets => assets;

        public TFilter[] Get<TFilter>() where TFilter : UnityEngine.Object => assets != null ? assets.OfType<TFilter>().ToArray() : Array.Empty<TFilter>();

        public bool HasAny<TFilter>() where TFilter : UnityEngine.Object => assets != null && assets.Any(tobject => tobject is TFilter);

        public int Count => assets?.Length ?? 0;

#if UNITY_EDITOR

        private static readonly (AssetFilter flag, string filter)[] TypeMap = new[]
        {
            (AssetFilter.Texture2D,     "t:Texture2D"),
            (AssetFilter.Sprite,        "t:Sprite"),
            (AssetFilter.AudioClip,     "t:AudioClip"),
            (AssetFilter.VideoClip,     "t:VideoClip"),
            (AssetFilter.TextAsset,     "t:TextAsset"),
            (AssetFilter.Material,      "t:Material"),
            (AssetFilter.Model,         "t:Model"),
            (AssetFilter.AnimationClip, "t:AnimationClip"),
            (AssetFilter.Shader,        "t:Shader"),
            (AssetFilter.Prefab,        "t:GameObject"),
            (AssetFilter.ScriptableObj, "t:ScriptableObject"),
            (AssetFilter.Mesh,          "t:Mesh"),
            (AssetFilter.RenderTexture, "t:RenderTexture"),
            (AssetFilter.Cubemap,       "t:Cubemap"),
            (AssetFilter.Texture3D,     "t:Texture3D"),
            (AssetFilter.Font,          "t:Font"),
        };


        private void OnValidate()
        {
            if (autoRefresh)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            if (folder == null)
            {
                assets = Array.Empty<UnityEngine.Object>();
                return;
            }

            var folderPath = AssetDatabase.GetAssetPath(folder);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                assets = Array.Empty<UnityEngine.Object>();
                return;
            }

            var selectedFilters = TypeMap.Where(selected => searchFilter.HasFlag(selected.flag))
                                          .Select(selected => selected.filter)
                                          .ToArray();

            if (selectedFilters.Length == 0 || searchFilter == AssetFilter.None)
            {
                assets = Array.Empty<UnityEngine.Object>();
                return;
            }

            var pathSet = new HashSet<string>();

            foreach (var filtered in selectedFilters)
            {
                foreach (var guid in AssetDatabase.FindAssets(filtered, new[] { folderPath }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    pathSet.Add(path);
                }
            }

            var paths = pathSet.AsEnumerable();
            if (!includeSubfolders)
            {
                string normalizedPath = folderPath.Replace('\\', '/');
                paths = paths.Where(path =>
                {
                    string directory = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
                    return string.Equals(directory, normalizedPath, StringComparison.Ordinal);
                });
            }

            var arrange = paths
                .Select(path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path))
                .Where(asset => asset != null);

            if (sortByName)
            {
                arrange = arrange.OrderBy(asset => asset.name, StringComparer.Ordinal);
            }

            assets = arrange.ToArray();
        }
#endif

        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize() { }
    }
}