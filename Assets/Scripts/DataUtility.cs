using System;
using System.Text;
using System.IO;
using UnityEngine;
using ICSharpCode.SharpZipLib.GZip;

public class DataUtility
{
    static readonly int BufferSize = 1024;

    static private string getRootPath()
    {
        return string.Format("{0}/{1}", Application.persistentDataPath, "data");
    }

    static public string[] getFolderList()
    {
        string[] firectoryNames = new string[0];
        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(getRootPath());
        if (di.Exists)
        {
            System.IO.DirectoryInfo[] files =
                di.GetDirectories("*", System.IO.SearchOption.AllDirectories);
            firectoryNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                firectoryNames[i] = files[i].Name;
            }
        }
        return firectoryNames;
    }

    // 保存
    static public void save<T>(T instance, string directory,string fileName)
    {
        var filepath = createFilePath(directory, fileName);

        // instance → JSON
        var json = JsonUtility.ToJson(instance);

        using (var outputStream = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.None))
        using (GZipOutputStream complessStream = new GZipOutputStream(outputStream))
        using (var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json)))

        {
            int byteCount;
            byte[] buffer = new byte[BufferSize];
            while ((byteCount = memoryStream.Read(buffer, 0, buffer.Length)) > 0)
                complessStream.Write(buffer, 0, byteCount);
        }
    }


    // 読み込み
    static public T load<T>(string directory, string fileName)
    {
        var filepath = createFilePath(directory, fileName);

        if (!File.Exists(filepath))
            return default(T);

        using (var inputStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.None)) // バイナリファイル → 圧縮データ
        using (GZipInputStream decomplessStream = new GZipInputStream(inputStream))
        using (var outputStream = new MemoryStream())
        {
            // 圧縮データ → MemoryStream
            int byteCount;
            byte[] buffer = new byte[BufferSize];
            while ((byteCount = decomplessStream.Read(buffer, 0, buffer.Length)) > 0)
                outputStream.Write(buffer, 0, byteCount);

            // MemoryStream → Json
            var json = Encoding.UTF8.GetString(outputStream.ToArray());

            // JSON → instance
            T instance = JsonUtility.FromJson<T>(json);

            return instance;
        }
    }
    static public void storePictureData(Texture2D texture,string directory,string fileName)
    {
        var filepath = createFilePath(directory, fileName);

        byte[] pngData = texture.EncodeToPNG();

        File.WriteAllBytes(filepath, pngData);
    }

    static public Texture2D readPictureData(string directory, string fileName)
    {
        var filepath = createFilePath(directory, fileName);

        FileStream fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] readBinary = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Close();

        int pos = 16; // 16バイトから開始

        int width = 0;
        for (int i = 0; i < 4; i++)
        {
            width = width * 256 + readBinary[pos++];
        }

        int height = 0;
        for (int i = 0; i < 4; i++)
        {
            height = height * 256 + readBinary[pos++];
        }

        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(readBinary);

        return texture;
    }
    static public string createFilePath(string directory, string fileName)
    {
        var folderpath = string.Format("{0}/{1}", getRootPath(), directory);
        Directory.CreateDirectory(folderpath);
        return string.Format("{0}/{1}", folderpath, fileName);
    }
}

