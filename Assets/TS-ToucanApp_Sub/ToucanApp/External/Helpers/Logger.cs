using System;
using System.IO;
using System.Linq;
using UnityEngine;
using ToucanApp.Data;

public class Logger : MonoBehaviour
{
    private const int MAX_LOGS_COUNT = 15;

    private string logs_path;
    private string log_file;
    private UploadHandler uploader;

    private void Awake()
    {
        uploader = GetComponentInParent<UploadHandler>();

        logs_path = Application.persistentDataPath + "/logs/";
        log_file = "file_" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + ".log";

        if (!Directory.Exists(logs_path))
            Directory.CreateDirectory(logs_path);

        using (FileStream fs = File.Create(logs_path + log_file)) { };

        var sortedFiles = new DirectoryInfo(logs_path).GetFiles().OrderBy(f => f.LastWriteTime).ToArray();
        int deleteFilesCount = Mathf.Max(0, sortedFiles.Length - MAX_LOGS_COUNT);

        for (int i = 0; i < deleteFilesCount; i++)
            File.Delete(sortedFiles[i].FullName);
    }

    private void WriteToLog(string log, string stackTrace, LogType logType)
    {
        var log_message = logType.ToString() + " -> " + log + Environment.NewLine;
        File.AppendAllText(logs_path + log_file, log_message);

        TrySendLog(log_message, logType);
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
            Application.logMessageReceived += WriteToLog;
    }

    private void OnDisable()
    {
        if (Application.isPlaying)
            Application.logMessageReceived -= WriteToLog;
    }

    private void TrySendLog(string log, LogType logType)
    {
        if (Application.isPlaying)
        {
            if (uploader != null && (logType == LogType.Error || logType == LogType.Exception))
            {
                uploader.UploadLogMessage(log);
            }
        }
    }
}
