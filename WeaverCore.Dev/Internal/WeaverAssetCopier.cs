using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Internal
{
	class WeaverAssetCopier : IInit
	{
		public void OnInit()
		{
			var sourceFolder = new DirectoryInfo("Assets\\WeaverCore");
			var destinationFolder = new DirectoryInfo("Assets\\WeaverCore\\Other Projects~\\Weaver Assets\\Assets\\WeaverCore");
            var destinationAssets = new DirectoryInfo("Assets\\WeaverCore\\Other Projects~\\Weaver Assets\\Assets");

            if (!destinationAssets.Exists)
            {
                return;
            }

            if (!destinationFolder.Exists)
            {
                destinationFolder.Create();
            }

            var sourceFiles = sourceFolder.GetFiles("*.*", SearchOption.AllDirectories).Where(f => !f.FullName.Contains("Other Projects~") && !f.FullName.Contains(".git") && !f.FullName.Contains("Hidden~\\CopyHashes.txt"));
            var sourceFilesRel = CreateRelativePaths(sourceFiles, sourceFolder.FullName);


            var destFiles = destinationFolder.GetFiles("*.*", SearchOption.AllDirectories);//.Where(f => !f.FullName.Contains("Other Projects~") && !f.FullName.Contains(".git") && !f.FullName.Contains("Hidden~\\CopyHashes.txt"));
            var destFilesRel = CreateRelativePaths(destFiles, destinationFolder.FullName);

            var differences = GetDifferences(sourceFilesRel, destFilesRel);
            foreach (var diff in differences)
            {
                File.Delete(destinationFolder.FullName + "\\" + diff);
            }

            var oldHashes = FileHashes.GetHashes();
            var newHashes = new Dictionary<string, string>();

            foreach (var file in sourceFilesRel)
            {
                var fullFilePath = sourceFolder.FullName + "\\" + file;
                var hash = StreamUtilities.GetHash(fullFilePath);

                bool overwriteFile = true;

                newHashes.Add(file, hash);

                if (oldHashes.ContainsKey(file))
                {
                    if (oldHashes[file] == hash)
                    {
                        overwriteFile = false;
                    }
                }

                if (overwriteFile)
                {
                    var destFullPath = destinationFolder.FullName + "\\" + file;
                    if (File.Exists(destFullPath))
                    {
                        File.Delete(destFullPath);
                    }

                    var copyDirectory = new FileInfo(destFullPath).Directory;

                    if (!copyDirectory.Exists)
                    {
                        copyDirectory.Create();
                    }
                    //fullFilePath = fullFilePath.Replace("/", "\\");
                    //destFullPath = destFullPath.Replace("/", "\\");
                    //WeaverLog.Log("COPYING FILE = " + fullFilePath);
                    //WeaverLog.Log("Destination = " + destFullPath);
                    File.Copy(fullFilePath, destFullPath);
                }
                /*else
                {
                    newHashes.Add(file, hash);
                }*/
            }

            FileHashes.SetHashes(newHashes);




            /*if (sourceFolder.Exists && destinationFolder.Exists)
			{

			}
			File.Copy()*/
        }

        static IEnumerable<string> GetDifferences(IEnumerable<string> source, IEnumerable<string> dest)
        {
            return dest.Where(d => !source.Any());
        }

        static List<string> CreateRelativePaths(IEnumerable<FileInfo> sourcePaths, string relativeTo)
        {
            List<string> relativePaths = new List<string>();
            foreach (var path in sourcePaths)
            {
                relativePaths.Add(PathUtilities.MakePathRelative(relativeTo,path.FullName));
            }
            return relativePaths;
        }

        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }

    [Serializable]
    class HashPair
    {
        public string file;
        public string hash;
    }


    [Serializable]
    class FileHashes
    {
        public static string hashesFileLocation = "Assets\\WeaverCore\\Hidden~\\CopyHashes.txt";
        [SerializeField]
        List<HashPair> hashPairs = new List<HashPair>();
        [NonSerialized]
        public Dictionary<string, string> Hashes;



        public static Dictionary<string, string> GetHashes()
        {
            var hashesFile = new FileInfo(hashesFileLocation);
            if (hashesFile.Exists)
            {
                var hashList = JsonUtility.FromJson<FileHashes>(File.ReadAllText(hashesFile.FullName));
                hashList.ConvertToDictionary();
                return hashList.Hashes;
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }

        public static void SetHashes(Dictionary<string, string> hashes)
        {
            var hashesFile = new FileInfo(hashesFileLocation);
            using (var file = File.CreateText(hashesFile.FullName))
            {
                var hashList = new FileHashes() { Hashes = hashes };
                hashList.ConvertToPairs();
                file.Write(JsonUtility.ToJson(hashList));
            }
        }

        void ConvertToDictionary()
        {
            Hashes = new Dictionary<string, string>();
            foreach (var pair in hashPairs)
            {
                Hashes.Add(pair.file, pair.hash);
            }
        }

        void ConvertToPairs()
        {
            hashPairs = new List<HashPair>();
            foreach (var pair in Hashes)
            {
                hashPairs.Add(new HashPair() { file = pair.Key, hash = pair.Value });
            }
        }
    }
}
