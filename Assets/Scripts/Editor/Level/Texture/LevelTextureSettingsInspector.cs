using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Extensions;
using Editor.AssetProcessors;
using UnityEngine;
using UnityEditor;

using Level.Texture;

namespace Editor.Level.Texture
{
    [CustomEditor(typeof(LevelTextureConfig))]
    public class LevelTextureSettingsInspector : UnityEditor.Editor
    {
        new LevelTextureConfig target => base.target as LevelTextureConfig;

        //bool _HashChanged;
        EAtlasType m_currentView = EAtlasType.Default;

        string[] m_viewOptions;

        const string k_textureAtlasDirectory = "Assets/Art/LevelTextureAtlas/";
        const string k_levelMaterialsDirectory = "Assets/Art/LevelMaterials/";

        string MaterialPath => $"{k_levelMaterialsDirectory}{target.name}.mat";
        //string Path
        //{
        //    get
        //    {
        //        //chop .asset
        //        var thisPath = AssetDatabase.GetAssetPath(target);
        //        return thisPath.Substring(0, thisPath.Length - 6);
        //    }
        //}

        void OnEnable()
        {
            m_viewOptions = new string[(int)EAtlasType.Length];
            var names = System.Enum.GetNames(typeof(EAtlasType));

            for (var i = 0; i < m_viewOptions.Length; i++)
                m_viewOptions[i] = names[i];
        }

        string ValidateTextureCollection(string collectionName, IReadOnlyCollection<TextureCollection> textureCollections)
        {
            var setting = target;
            if (textureCollections == null || textureCollections.Count == 0)
                return $"No {collectionName} texture collections defined";

            var tileDimension = 2 * setting.HalfTileDimension;
            if (tileDimension <= 0 || !Mathf.IsPowerOfTwo(setting.HalfTileDimension))
                return $"Invalid TileDimension in {setting.name}";
            if (setting.AtlasDimension <= tileDimension || !Mathf.IsPowerOfTwo(setting.AtlasDimension))
                return $"Invalid AtlasDimension in {setting.name}";

            foreach (var textureCollection in textureCollections)
            {
                if (textureCollection == null)
                    return $"There's a null texture {collectionName} collection.";

                if (!textureCollection.Any())
                    return $"No textures in {collectionName} collection '{textureCollection.Name}'";

                foreach (var dat in textureCollection)
                {
                    ValidateTexture(collectionName, dat, textureCollection, tileDimension, out var eMessage);
                    if (eMessage != null)
                        return eMessage;
                }
            }

            return null;
        }

        static void ValidateTexture(string collectionName, TextureData dat, TextureCollection textureCollection,
            int tileDimension, out string eMessage)
        {
            // todo: normal textures
            var texture = dat.GetTexture();
            if (texture == null)
            {
                eMessage = $"There's a null texture in {collectionName} collection '{textureCollection.Name}'";
                return;
            }

            if (texture.width < tileDimension || texture.height < tileDimension)
            {
                eMessage = $"The texture {texture.name} in {collectionName} collection '{textureCollection.Name}' is too small";
                return;
            }

            if (!Mathf.IsPowerOfTwo(texture.width) || !Mathf.IsPowerOfTwo(texture.height))
            {
                eMessage = $"The texture {texture.name} in {collectionName} collection '{textureCollection.Name}' is not power of two!";
                return;
            }

            if (texture.width % tileDimension != 0 || texture.width % tileDimension != 0)
            {
                eMessage = $"The texture {texture.name} in {collectionName} collection '{textureCollection.Name}' is not multiple of the tileDimension {tileDimension}!";
                return;
            }

            if (dat.Type == ETexType.Invalid)
            {
                eMessage = $"The texture {texture.name} in {collectionName} collection '{textureCollection.Name}' has invalid TexType (wrong dimensions)!";
                return;
            }

            EnsureTextureReadableAndUncompressed(out eMessage, texture);
        }

        static void EnsureTextureReadableAndUncompressed(out string eMessage, Texture2D texture)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null &&
                (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed))
            {
                eMessage = "Texture " + texture.name + " must be readable and uncompressed";
                return;
            }

            try
            {
                texture.GetPixel(0, 0);
            }
            catch (UnityException e)
            {
                eMessage = e.Message;
                return;
            }
            eMessage = null;
        }

        string Validate()
        {
            // todo: collectionName: "Ground: probably obsolete (see below "Mask")
            var warning = ValidateTextureCollection("Ground", target.LevelTextureCollections);
            //if (warning!= null)
            return warning;

            //warning = ValidateTextureCollection("Mask", target.MaskTextureCollections);
            //if (warning!= null)
            //    return warning;

            //return null;
        }

        void UpdateAtlasIfNeeded()
        {
            var setting = target;
            var recentImported = LevelTexturePreprocessor.PullRecentlyImportedTextures();

            var updateNeeded = false;
            foreach (var newTex in recentImported)
            {
                if (!setting.Contains(newTex))
                    continue;

                var newTexHash = LevelTexturePreprocessor.GetHash(newTex);
                if (setting.UsedTexturePixelHashes.Contains(newTexHash))
                    continue;

                updateNeeded = true;
                break;
            }

            if (updateNeeded)
                GenerateAtlas();
        }

        public override void OnInspectorGUI()
        {
            var warnings = Validate();
            var hasWarnings = !string.IsNullOrEmpty(warnings);
            if (hasWarnings)
                EditorGUILayout.HelpBox(warnings, MessageType.Warning);

            // todo: validation checks
            //if (_HashChanged)
            //    EditorGUILayout.HelpBox("The Settings have changed. Please check all rooms if they look good (implement feature to list all rooms using atlas)", MessageType.Info);

            using (new EditorGUI.DisabledScope(hasWarnings))
            {
                if (GUILayout.Button("Generate Atlas", GUILayout.Height(35)))
                {
                    GenerateAtlas();
                }
            }

            serializedObject.Update();

            LevelTextureSettingGUI();

            if (target.ActiveMaps[(int)m_currentView])
            {
                TextureCollectionGUI("Texture Types", $"{nameof(LevelTextureConfig.LevelTextureCollections)}", ref target.LevelTextureCollections);
                //TextureCollectionUI("Mask Texture Collections", "MaskTextureCollections", ref target.MaskTextureCollections);
                TransitionsGUI("Transitions", target.Transitions);
            }

            UpdateAtlasIfNeeded();
        }

        void LevelTextureSettingGUI()
        {
            var settings = target;
            settings.AtlasDimension = EditorGUILayout.IntField("AtlasDimension", settings.AtlasDimension);
            settings.HalfTileDimension = EditorGUILayout.IntField("HalfTileDimension", settings.HalfTileDimension);

            m_currentView = (EAtlasType)EditorGUILayout.Popup("CurrentView: ", (int)m_currentView, m_viewOptions);
            if (m_currentView == EAtlasType.Length)
                m_currentView = EAtlasType.Default;
            settings.ActiveMaps[(int)m_currentView] = EditorGUILayout.Toggle("Active", settings.ActiveMaps[(int)m_currentView]);
        }

        void SingleTexDataGUI(TextureData dat)
        {
            Texture2D currentTex = null;
            float w = 50.0f, h = 50.0f;
            if (dat.IsSet())
            {
                currentTex = dat.GetTexture(m_currentView);
                var dim = dat.TileDimension();
                w = 50.0f * dim.x;
                h = 50.0f * dim.y;
            }

            var rect = EditorGUILayout.GetControlRect(GUILayout.Width(w), GUILayout.Height(h + 10.0f));
            var newTex = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 10.0f, w, h), currentTex, typeof(Texture2D), false) as Texture2D;
            if (currentTex != newTex)
            {
                dat.SetTexture(m_currentView, newTex);
                target.UpdateCollectionDataTypes();
            }

            if (GUI.Button(new Rect(rect.x, rect.y, w, 10.0f), "X"))
            {
                dat.SetTexture(m_currentView, null);
            }
        }

        void TexDataListGUI(IList<TextureData> data, string label)
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(50.0f));

                var removeIdx = -1;
                for (var j = 0; j < data.Count; ++j)
                {
                    var rect = EditorGUILayout.GetControlRect(GUILayout.Width(50.0f), GUILayout.Height(60.0f));
                    var tex = data[j].GetTexture(m_currentView);
                    var newTex = EditorGUI.ObjectField(new Rect(rect.x, rect.y + 10.0f, 50.0f, 50.0f), tex, typeof(Texture2D), false) as Texture2D;
                    if (newTex != tex)
                    {
                        data[j].SetTexture(m_currentView, newTex);
                        target.UpdateCollectionDataTypes();
                    }
                    if (m_currentView == EAtlasType.Default && GUI.Button(new Rect(rect.x, rect.y, 50.0f, 10.0f), "X"))
                        removeIdx = j;
                }
                if (GUILayout.Button("+", GUILayout.Width(20.0f)))
                    data.Add(new TextureData());

                if (removeIdx != -1)
                {
                    data.RemoveAt(removeIdx);
                }
            }
        }

        void UpdateTexCollectionNames()
        {
            var setting = target;
            m_textureCollectionNames = new string[setting.LevelTextureCollections.Count];
            for (int i = 0; i < setting.LevelTextureCollections.Count; ++i)
            {
                m_textureCollectionNames[i] = setting.LevelTextureCollections[i].Name;
            }
        }

        void UpdateTransitionNames()
        {
            var setting = target;
            UpdateTexCollectionNames();
            setting.UpdateTransitionNames();
        }

        void UpdateTransitionIndices()
        {
            var setting = target;
            UpdateTexCollectionNames();
            setting.UpdateTransitionIndices();
        }

        string[] m_textureCollectionNames;

        void TransitionsGUI(string label, List<TransitionTextureData> transitions)
        {
            if (m_textureCollectionNames == null)
                UpdateTransitionNames();

            EditorGUILayout.LabelField(label);

            var erasedTransition = -1;
            for (var i = 0; i < transitions.Count; ++i)
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("X", GUILayout.Width(20.0f)))
                        erasedTransition = i;

                    SelectTransitionFromToTypesGUI(transitions, i);

                    SelectTransitionTextureGUI(transitions, i);
                }
            }
            if (GUILayout.Button("Add Transition"))
                AddTransition(transitions);
            
            if (erasedTransition != -1)
                transitions.RemoveAt(erasedTransition);
        }

        void SelectTransitionTextureGUI(IReadOnlyList<TransitionTextureData> transitions, int i)
        {
            Texture2D currentTex = null;
            // float w = 20.0f, h = 20.0f;
            if (transitions[i].IsSet())
                currentTex = transitions[i].GetTexture();

            if (m_currentView == EAtlasType.Default &&
                GUILayout.Button("X"))
            {
                transitions[i].SetTexture(EAtlasType.Default, null);
            }

            var newTex = EditorGUILayout.ObjectField(currentTex, typeof(Texture2D), false) as Texture2D;
            if (currentTex == newTex)
                return;

            transitions[i].SetTexture(m_currentView, newTex);
            target.UpdateCollectionDataTypes();
        }

        void SelectTransitionFromToTypesGUI(IReadOnlyList<TransitionTextureData> transitions, int i)
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                transitions[i].TexAIdx = EditorGUILayout.Popup(transitions[i].TexAIdx, m_textureCollectionNames);
                transitions[i].TexBIdx = EditorGUILayout.Popup(transitions[i].TexBIdx, m_textureCollectionNames);
                if (!check.changed)
                    return;
                transitions[i].TexAName = m_textureCollectionNames[transitions[i].TexAIdx];
                transitions[i].TexBName = m_textureCollectionNames[transitions[i].TexBIdx];
            }
        }

        void AddTransition(ICollection<TransitionTextureData> transitions)
        {
            var data = new TransitionTextureData
            {
                TexAName = m_textureCollectionNames[0],
                TexBName = m_textureCollectionNames[0]
            };
            transitions.Add(data);
        }

        void TextureCollectionGUI(string label, string propertyName, ref List<TextureCollection> collection)
        {
            EditorGUILayout.LabelField(label);
            var groundTexProp = serializedObject.FindProperty(propertyName);

            var erasedTexCollection = -1;
            for (var i = 0; i < collection.Count; ++i)
            {
                TextureCollectionElementGUI(collection, groundTexProp, i, ref erasedTexCollection);
            }

            if (GUILayout.Button("Add Texture Collection"))
            {
                collection.Add(new TextureCollection());
            }
            if (erasedTexCollection != -1)
            {
                collection.RemoveAt(erasedTexCollection);
            }
        }

        void TextureCollectionElementGUI(IReadOnlyList<TextureCollection> collection, SerializedProperty groundTexProp, int i, 
            ref int erasedTexCollection)
        {
            if (groundTexProp.arraySize <= i)
                return;
            var propEl = groundTexProp.GetArrayElementAtIndex(i);

            TextureCollectionElementHeaderGUI(collection, i, propEl, ref erasedTexCollection);

            if (propEl == null || !propEl.isExpanded)
                return;

            TextureCollectionElementEdgeOptionsGUI(collection, i);

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Edge:", GUILayout.Width(50.0f));
                using (new GUILayout.VerticalScope())
                {
                    TextureCollectionElementEdgeTextureGUI(collection, i);
                    TextureCollectionElementDiagonalEdgeTextureGUI(collection, i);
                }

                using (new GUILayout.VerticalScope())
                {
                    TexDataListGUI(collection[i].FloorVariations, "Floor:");
                    TexDataListGUI(collection[i].WallVariations, "Walls:");
                }
            }

            GUILayout.Space(20.0f);
        }

        void TextureCollectionElementDiagonalEdgeTextureGUI(IReadOnlyList<TextureCollection> collection, int i)
        {
            if (collection[i].Edge.Type != ETexType.HardEdgeSq)
                return;
            using (var cs = new EditorGUI.ChangeCheckScope())
            {
                SingleTexDataGUI(collection[i].DiagEdge);
                if (!cs.changed)
                    return;
                if (collection[i].HasDiagEdge)
                    collection[i].ForceHardEdge = false;
                else if (collection[i].Edge.Type == ETexType.HardEdgeSq)
                    collection[i].ForceHardEdge = true;
            }
        }

        void TextureCollectionElementEdgeTextureGUI(IReadOnlyList<TextureCollection> collection, int i)
        {
            using (var cs = new EditorGUI.ChangeCheckScope())
            {
                SingleTexDataGUI(collection[i].Edge);
                if (!cs.changed)
                    return;
                if (collection[i].Edge.Type == ETexType.HardEdgeSq)
                {
                    if (!collection[i].HasDiagEdge)
                        collection[i].ForceHardEdge = true;
                }
                else if (collection[i].HasDiagEdge)
                {
                    collection[i].DiagEdge.SetTexture(EAtlasType.Default, null);
                }
            }
        }

        static void TextureCollectionElementEdgeOptionsGUI(IReadOnlyList<TextureCollection> collection, int i)
        {
            var forceHardEditDisabled = collection[i].HasDiagEdge ||
                                        (!collection[i].HasDiagEdge && collection[i].Edge.Type == ETexType.HardEdgeSq);

            using (new EditorGUI.DisabledScope(forceHardEditDisabled))
            {
                collection[i].ForceHardEdge = EditorGUILayout.Toggle("Force hard edge", collection[i].ForceHardEdge);
            }

            if (collection[i].Edge.IsSet())
                collection[i].UseEdgeOnlyForTexTypeChanges = EditorGUILayout.Toggle("Use Edge Only For TexType changes",
                    collection[i].UseEdgeOnlyForTexTypeChanges);
        }

        void TextureCollectionElementHeaderGUI(IReadOnlyList<TextureCollection> collection, int i, SerializedProperty propEl, 
            ref int erasedTexCollection)
        {
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("X", GUILayout.Width(20.0f)))
                {
                    UpdateTransitionIndices();
                    erasedTexCollection = i;
                }

                GUILayout.Space(10.0f);
                if (propEl != null)
                    propEl.isExpanded = EditorGUILayout.Foldout(propEl.isExpanded, collection[i].Name, true);

                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    collection[i].Name = EditorGUILayout.TextField(collection[i].Name);
                    if (check.changed)
                        UpdateTransitionNames();
                }
            }
        }

        void GenerateAtlas()
        {
            Debug.Assert(Validate() == null);

            var texAtlasDir = Path.GetDirectoryName(k_textureAtlasDirectory);
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!texAtlasDir.IsNullOrEmpty() && !Directory.Exists(texAtlasDir))
                Directory.CreateDirectory(texAtlasDir);
            var materialsDir = Path.GetDirectoryName(k_levelMaterialsDirectory);
            // ReSharper disable once AssignNullToNotNullAttribute
            if (!materialsDir.IsNullOrEmpty() && !Directory.Exists(materialsDir))
                Directory.CreateDirectory(materialsDir);

            AtlasGeneration.GenerateAtlas(target, MaterialPath, k_textureAtlasDirectory);
        }
    }
}