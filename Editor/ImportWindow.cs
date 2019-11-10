﻿using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor {
    /*
     * Editor window/tab for importing WoW map tiles from Marlamin's WoW Exporter.
     *
     * Tested with version 0.3.2.4.
     */
    public class ImportWindow : EditorWindow {

        private TextAsset _definitionFile;
        private string _assetPath;

        [MenuItem("Window/WoW Tile Importer")]
        public static void ShowWindow() {
            var importWindow = GetWindow(typeof(ImportWindow));
            importWindow.titleContent.text = "WoW Tile Importer";
            importWindow.Show();
        }

        private void OnGUI() {
            GUILayout.Label("Import single tile CSV", EditorStyles.boldLabel);
            _definitionFile =
                (TextAsset) EditorGUILayout.ObjectField("CSV file", _definitionFile, typeof(TextAsset), false);
            if (GUILayout.Button("Import")) {
                StartImport();
            }

            GUILayout.Label("Import all tiles from path", EditorStyles.boldLabel);
            _assetPath = EditorGUILayout.TextField("Asset path", _assetPath);
            if (GUILayout.Button("Import")) {
                StartImportFromPath();
            }
        }

        private void StartImport() {
            var tileImporter = CreateInstance<TileImporter>();
            tileImporter.Import(new[] {_definitionFile});
        }

        private void StartImportFromPath() {
            var tilesAssets = FindTiles(_assetPath);
            var tileImporter = CreateInstance<TileImporter>();
            tileImporter.Import(tilesAssets);
        }

        private static TextAsset[] FindTiles(string basePath) {
            var worldName = Path.GetFileName(basePath);
            return AssetDatabase.FindAssets("t:TextAsset " + worldName + "_?_?_ModelPlacementInformation",
                    new[] {basePath}).Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<TextAsset>).ToArray();
        }

    }

}