using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// v.02b no crypto, simplifications, no backup

[System.Serializable]
public abstract class SavableJson<T> where T : SavableJson<T>, new()
{
    public abstract string fileName { get; }

    public static T instance
    {
        get
        {
            if (_instance == null) _instance = Load();
            return _instance;
        }
    }

    static T _instance;

    public static void SaveInstance()
    {
        instance.Save();
    }

    public void Save(string fileName = null)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            fileName = GetPersistentPath(this.fileName);
        }

        SaveAbs(fileName);
    }

    public void SaveAbs(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.Log("vrr");
            fileName = this.fileName;
        }

        var asString = JsonUtility.ToJson(this, true);
        WriteAllText(fileName, asString);
        // WriteAllTextWithBackup(fileName, asString);
        Debug.Log("Saved To " + fileName);
    }

    public static bool Exists(string fileName)
    {
        return (File.Exists(GetPersistentPath(fileName)));
    }

    public static string RemoveInvalidChars(string filename)
    {
        return string.Concat(filename.Split(Path.GetInvalidFileNameChars()));
    }

    /// <summary>
    /// Adds persistent path
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static T Load()
    {
        T temp = new T();
        string fileName = GetPersistentPath(temp.fileName);
        // Debug.Log($"Load invoked {fileName}");
        if (!Exists(fileName))
        {
            Debug.Log($"{fileName} file not found returning new " + typeof(T).ToString());
            temp.Prepare();
            temp.Save(); // auto save?
            return temp;
        }

        var asString = File.ReadAllText(fileName);
        if (string.IsNullOrEmpty(asString))
        {
            Debug.Log($"read string is null or empty {fileName}, returning new");
            temp.Prepare();
            return temp;
        }

        var pr = JsonUtility.FromJson<T>(asString);
        pr.Prepare();
        return pr as T;
    }

    public static T LoadAbs(string fileName) // where T:SavableJson
    {
        if (!File.Exists(fileName))
        {
            var prr = default(T);
            Debug.Log("failed loading " + fileName);
            if (prr != null)
                prr.Prepare();

            return prr;
        }

        var asString = File.ReadAllText(fileName);
        // if (CryptoExt.encrypt)
        //     asString = StringCipher.Decrypt(asString, CryptoExt.password);
        var pr = JsonUtility.FromJson<T>(asString);
        if (pr == null) pr = default(T);
        pr.Prepare();
        return pr as T;
    }

    public static string GetPersistentPath(string fileName)
    {
        //fileName = fileName.Deslash();
        // Debug.Log("filename =" + fileName);
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public virtual void Prepare()
    {
    }

    public static void WriteAllText(string path, string contents)
    {
        Debug.Log($"file did not exist, moving to {path}");
        File.WriteAllText(path, contents);
    }

    public static void WriteAllTextWithBackup(string path, string contents)
    {
        // generate a temp filename
        var tempPath = path + ".temp";
        // create the backup name
        // delete any existing backups
        // get the bytes
        var data = Encoding.UTF8.GetBytes(contents);

        // write the data to a temp file
        using (var tempFile = File.Create(tempPath, 4096, FileOptions.WriteThrough))
            tempFile.Write(data, 0, data.Length);

        //File.SetCreationTime(tempPath,new System.DateTime(1990,03,03) );  

        var backup = path + ".backup";
        if (File.Exists(backup))
        {
            Debug.Log($"backup  {backup} existed, deleting");
            File.Delete(backup);
        }

        if (File.Exists(path))
        {
            Debug.Log($"file {path} did  exist, replacing");
            File.Replace(tempPath, path, backup);
        }
        else
        {
            Debug.Log($"file did not exist, moving to {path}");
            File.Move(tempPath, path);
        }
    }
}