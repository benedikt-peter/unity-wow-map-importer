﻿using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor {

    /**
     * This class allows to import one or more tiles.
     */
    internal class TileImporter : ScriptableObject {

        private const float Offset = (float) (51200.0d / 3.0d);

        private TextAsset[] _definitionFiles;

        private string _basePath;
        private string _prefabsPath;
        private string _materialsPath;

        private HashSet<ObjectKey> _objects;

        public void Import(TextAsset[] definitionFiles) {
            if (definitionFiles.Length < 1) {
                return;
            }

            _definitionFiles = definitionFiles;
            var assetPath = AssetDatabase.GetAssetPath(_definitionFiles[0]);
            _basePath = Directory.GetParent(assetPath).ToString();

            _prefabsPath = _basePath + "/Prefabs";
            if (!AssetDatabase.IsValidFolder(_prefabsPath)) {
                AssetDatabase.CreateFolder(_basePath, "Prefabs");
            }

            _materialsPath = _basePath + "/Materials";
            if (!AssetDatabase.IsValidFolder(_materialsPath)) {
                AssetDatabase.CreateFolder(_materialsPath, "Materials");
            }

            _objects = new HashSet<ObjectKey>();
            DoImport();
        }

        private void DoImport() {
            foreach (var file in _definitionFiles) {
                DoImportFile(file);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private void DoImportFile(TextAsset file) {
            var title = "Importing tile from file '" + file.text + "'";
            EditorUtility.DisplayProgressBar(title, "Importing terrain data...", 0.0f);

            var tileName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(file))
                .Replace("_ModelPlacementInformation", "");

            var root = new GameObject(tileName);

            DoImportTile(tileName, root);

            using (var stream = new MemoryStream(file.bytes))
            using (var reader = new StreamReader(stream)) {
                reader.ReadLine();

                var items = new List<TileItem>();

                string line;
                while ((line = reader.ReadLine()) != null) {
                    items.Add(TileItem.FromCsv(line));
                }

                for (var i = 0; i < items.Count; ++i) {
                    var tileItem = items[i];

                    EditorUtility.DisplayProgressBar(title, "Importing asset '" + tileItem.ModelFile + "'...",
                        (float) i / items.Count);

                    if (tileItem.Type == "wmo") {
                        DoImportWmo(tileItem, root);
                    } else {
                        DoImportAsset(tileItem, root);
                    }
                }
            }

            EditorUtility.DisplayProgressBar(title, "Refreshing asset database...", 0.99f);
            AssetDatabase.Refresh();
            EditorUtility.DisplayProgressBar(title, "Saving unsaved assets...", 0.99f);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayProgressBar(title, "Saving prefab...", 0.99f);
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, _prefabsPath + "/" + tileName + ".prefab",
                InteractionMode.AutomatedAction);
            EditorUtility.ClearProgressBar();
        }

        private void DoImportTile(string tileName, GameObject parent) {
            var instance = PrepareAsset(tileName + ".obj", parent);

            if (instance == null) {
                Debug.LogFormat("Failed to load tile '{0}'", tileName + ".obj");
            }

            instance.transform.rotation = Quaternion.Euler(0.0f, 180.0f, 0.0f);
        }

        private GameObject DoImportAsset(TileItem tileItem, GameObject parent) {
            var position = new Vector3(Offset - tileItem.PositionX, tileItem.PositionY,
                (Offset - tileItem.PositionZ) * -1.0f);
            var key = new ObjectKey(tileItem.ModelFile, position);

            if (!_objects.Add(key)) {
                Debug.LogFormat("Skipped duplicate asset '{0}' at position {1}", tileItem.ModelFile, position);
                return null;
            }

            var instance = PrepareAsset(tileItem.ModelFile, parent);

            if (instance == null) {
                Debug.LogFormat("Failed to load asset '{0}'", tileItem.ModelFile);
                return null;
            }

            instance.transform.localPosition = position;
            instance.transform.localRotation =
                Quaternion.Euler(tileItem.RotationX, 90 + tileItem.RotationY, tileItem.RotationZ);
            instance.transform.localScale =
                new Vector3(tileItem.ScaleFactor, tileItem.ScaleFactor, tileItem.ScaleFactor);

            return instance;
        }

        private void DoImportWmo(TileItem tileItem, GameObject parent) {
            var instance = DoImportAsset(tileItem, parent);

            if (instance == null) {
                return;
            }

            var itemName = Path.GetFileNameWithoutExtension(tileItem.ModelFile);
            var wmoFile = _basePath + "/" + itemName + "_ModelPlacementInformation.csv";

            if (!File.Exists(wmoFile)) {
                Debug.LogFormat("Error: Could not find model placement file for WMO '{0}'", wmoFile);
            }

            using (var stream = new MemoryStream(AssetDatabase.LoadAssetAtPath<TextAsset>(wmoFile).bytes))
            using (var reader = new StreamReader(stream)) {
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null) {
                    var wmoItem = WmoItem.FromCsv(line);

                    DoImportWmoAsset(wmoItem, instance);
                }
            }

            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);
            instance.transform.Rotate(0.0f, 90.0f, 0.0f);
        }

        private void DoImportWmoAsset(WmoItem wmoItem, GameObject parent) {
            var instance = PrepareAsset(wmoItem.ModelFile, parent);

            if (instance == null) {
                Debug.LogFormat("Failed to load asset '{0}'", wmoItem.ModelFile);
                return;
            }

            instance.transform.parent = parent.transform;
            instance.transform.localPosition = new Vector3(wmoItem.PositionX, wmoItem.PositionZ,
                wmoItem.PositionY);
            var rotation = new Quaternion(wmoItem.RotationX, wmoItem.RotationY, wmoItem.RotationZ,
                wmoItem.RotationW).eulerAngles;
            instance.transform.localRotation = Quaternion.Euler(rotation.x, rotation.z, rotation.y);
            instance.transform.localScale = new Vector3(wmoItem.ScaleFactor, wmoItem.ScaleFactor, wmoItem.ScaleFactor);
        }

        private GameObject PrepareAsset(string assetName, GameObject parent) {
            var prefabPath = _prefabsPath + "/" + assetName + ".prefab";

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            GameObject instance;

            if (prefab == null) {
                instance = PrepareNewAsset(assetName, prefabPath);
            } else {
                instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (instance == null) {
                    return null;
                }
            }

            instance.transform.parent = parent.transform;
            return instance;
        }

        private GameObject PrepareNewAsset(string assetName, string prefabPath) {
            GameObject instance;
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(_basePath + "/" + assetName);
            instance = Instantiate(asset);
            instance.name = assetName;

            foreach (var meshRenderer in instance.GetComponentsInChildren<MeshRenderer>()) {
                PrepareMaterials(meshRenderer);
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(instance, prefabPath, InteractionMode.AutomatedAction);
            return instance;
        }

        private void PrepareMaterials(MeshRenderer meshRenderer) {
            var materials = meshRenderer.sharedMaterials;
            for (var i = 0; i < materials.Length; ++i) {
                materials[i] = PrepareMaterial(materials[i]);
            }

            meshRenderer.sharedMaterials = materials;
        }

        private Material PrepareMaterial(Material oldMaterial) {
            if (oldMaterial == null) {
                return null;
            }

            var materialFile = _materialsPath + "/" + oldMaterial.name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialFile);
            if (material == null) {
                AssetDatabase.ExtractAsset(oldMaterial, materialFile);
                material = AssetDatabase.LoadAssetAtPath<Material>(materialFile);
            }

            material.EnableKeyword("_ALPHABLEND_ON");
            material.SetFloat("_Mode", 2.0f);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

    }

}