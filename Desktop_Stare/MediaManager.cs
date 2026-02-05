using Desktop_Stare.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Windows.Media;

public static class MediaManager
{
    private static Dictionary<string, DateTime> LastPlayed = new Dictionary<string, DateTime>();
    private static Dictionary<string, SoundPlayer> players = new Dictionary<string, SoundPlayer>();
    private static Random rng = new Random();
    public static void PlaySound(string fileName,double delaySeconds = 0)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds","Doto",fileName);

        if (!File.Exists(path))
        {
            return;
        }
        if (delaySeconds > 0 && LastPlayed.TryGetValue(fileName, out DateTime lastTime) && (DateTime.Now - lastTime).TotalSeconds < delaySeconds)
        {
            return;
        }
        else
        {
            PlaySoundPlayer(path);
        }
        LastPlayed[fileName] = DateTime.Now;
    }
    private static void PlaySoundPlayer(string path)
    {
        if (!players.TryGetValue(path, out SoundPlayer sp))
        {
            sp = new SoundPlayer(path);
            players[path] = sp;
        }
        sp.Play();
    }
    public static void PlayRandomSoundFromFolder()
    {
        string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sounds", "Randoms");

        if (!Directory.Exists(folderPath))
        {
            return;
        } 
        var wavFiles = Directory.GetFiles(folderPath, "*.wav");
        if (wavFiles.Length == 0)
        {
            return;
        }
        string filePath = wavFiles[rng.Next(wavFiles.Length)];
        string fileName = Path.GetFileName(filePath);
        PlaySoundPlayer(filePath);
        LastPlayed[fileName] = DateTime.Now;
    }
}


