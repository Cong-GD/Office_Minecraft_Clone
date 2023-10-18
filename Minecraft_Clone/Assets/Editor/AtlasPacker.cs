using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;
using System;

public class AtlasPacker : EditorWindow
{
    int blockSize = 16; // Block size in pixels
    int atlasSizeInBlock = 16;
    int atlasSize;

    List<Texture2D> rawTexture;
    Texture2D atlas;

    [MenuItem("Minecraft Clone/Atlas Packer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AtlasPacker));
    }

    private void OnGUI()
    {
        atlasSize = blockSize * atlasSizeInBlock;

        GUILayout.Label("Minecraft Clone Texture Atlas Packer", EditorStyles.boldLabel);

        blockSize = EditorGUILayout.IntField("Block Size", blockSize);
        atlasSizeInBlock = EditorGUILayout.IntField("Atlas Size In Block", atlasSizeInBlock);

        GUILayout.Label(atlas);

        if (GUILayout.Button("Load Textures"))
        {
            LoadTextures();
            PackAtlas();
        }

        if(GUILayout.Button("Clear Textures"))
        {
            atlas = null;
            rawTexture = null;
        }

        if(atlas != null && GUILayout.Button("Save Atlas"))
        {
            byte[] bytes = atlas.EncodeToPNG();

            try
            {
                File.WriteAllBytes(Path.Combine(Application.dataPath, "Textures", "Packed_Atlas.png"), bytes);
                //var texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Textures/Packed_Atlas.png");
                //texture.alphaIsTransparency = true;
                //Debug.Log(texture.name);
            }
            catch
            {
                Debug.LogError("Atlas Packer: Couldn't save atlas to file");
            }
        }
    }

    void LoadTextures()
    {
        rawTexture = Resources.LoadAll<Texture2D>("Blocks")
                    .Where(texture =>
                    {
                        if(texture.width != blockSize || texture.height != blockSize)
                        {
                            Debug.LogWarning("Incorect texture size name: " + texture.name);
                            return false;
                        }
                        return true;
                    })
                    .OrderBy(texture => int.Parse(texture.name.Split("_")[0]))
                    .ToList();
    }

    void PackAtlas()
    {
        atlas = new Texture2D(atlasSize, atlasSize);
        Color[] pixels = new Color[atlasSize * atlasSize];

        for(int x = 0; x < atlasSize; x++)
        {
            for(int y = 0; y < atlasSize; y++)
            {

                int currentBlockX = x / blockSize;
                int currentBlockY = y / blockSize;

                int index = currentBlockY * atlasSizeInBlock + currentBlockX;

                if (index < rawTexture.Count)
                {
                    pixels[y * atlasSize + x] = rawTexture[index].GetPixel(x, y);
                }
                else
                {
                    pixels[y * atlasSize + x] = new Color(0, 0, 0, 0);
                }
            }
        }

        atlas.SetPixels(pixels);
        atlas.Apply();
    }
}
