#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using WeaverCore;

[InitializeOnLoad]
internal static class ForceAssetRefresh
{
	private const string PrefEnabledKey = "HK.ForceAssetRefreshOnChange.Enabled";
	private const string PrefVerboseKey = "HK.ForceAssetRefreshOnChange.Verbose";
	private const double DebounceSeconds = 0.35;
	private const double SuppressSecondsAfterRefresh = 1.0;

	private static readonly object StateLock = new object();
	private static readonly object InitLock = new object();
	private static FileSystemWatcher _assetsWatcher;
	private static FileSystemWatcher _packagesWatcher;
	private static bool _pending;
	private static long _lastEventTimestamp;
	private static string _lastPath;
	private static long _suppressEventsUntilTimestamp;

	private static readonly string ProjectRoot;
	private static readonly string AssetsRoot;
	private static readonly string PackagesRoot;

	static ForceAssetRefresh()
	{
		ProjectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
		AssetsRoot = Path.GetFullPath(Application.dataPath);
		PackagesRoot = Path.GetFullPath(Path.Combine(ProjectRoot, "Packages"));

		EditorApplication.delayCall += TryInitialize;
	}

	[MenuItem("WeaverCore/Tools/Asset Refresh/External File Watcher", priority = 2000)]
	private static void ToggleWatcherMenu()
	{
		EditorPrefs.SetBool(PrefEnabledKey, !IsEnabled());
		TryInitialize();
	}

	[MenuItem("WeaverCore/Tools/Asset Refresh/External File Watcher", true)]
	private static bool ToggleWatcherMenuValidate()
	{
		Menu.SetChecked("Tools/Asset Refresh/External File Watcher", IsEnabled());
		return true;
	}

	[MenuItem("WeaverCore/Tools/Asset Refresh/Refresh Now %F12", priority = 2010)]
	private static void RefreshNowMenu()
	{
		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
	}

	[MenuItem("WeaverCore/Tools/Asset Refresh/Verbose Logging", priority = 2020)]
	private static void ToggleVerboseLoggingMenu()
	{
		EditorPrefs.SetBool(PrefVerboseKey, !IsVerbose());
	}

	[MenuItem("WeaverCore/Tools/Asset Refresh/Verbose Logging", true)]
	private static bool ToggleVerboseLoggingMenuValidate()
	{
		Menu.SetChecked("Tools/Asset Refresh/Verbose Logging", IsVerbose());
		return true;
	}

	private static bool IsEnabled()
	{
		if (EditorPrefs.HasKey(PrefEnabledKey))
		{
			return EditorPrefs.GetBool(PrefEnabledKey, true);
		}

		return Application.platform == RuntimePlatform.LinuxEditor;
	}

	private static bool IsVerbose() => EditorPrefs.GetBool(PrefVerboseKey, false);

	private static void TryInitialize()
	{
		lock (InitLock)
		{
			if (!IsEnabled() || Application.platform != RuntimePlatform.LinuxEditor)
			{
				Dispose();
				return;
			}

			if (_assetsWatcher != null)
			{
				return;
			}

			_assetsWatcher = MakeWatcher(AssetsRoot);
			if (Directory.Exists(PackagesRoot))
			{
				_packagesWatcher = MakeWatcher(PackagesRoot);
			}

			EditorApplication.update += OnEditorUpdate;
			EditorApplication.focusChanged += OnFocusChanged;
			AssemblyReloadEvents.beforeAssemblyReload += Dispose;
			EditorApplication.quitting += Dispose;
		}
	}

	private static FileSystemWatcher MakeWatcher(string root)
	{
		var watcher = new FileSystemWatcher(root)
		{
			IncludeSubdirectories = true,
			NotifyFilter =
				NotifyFilters.FileName |
				NotifyFilters.DirectoryName |
				NotifyFilters.LastWrite |
				NotifyFilters.Size
		};

		watcher.Changed += OnFsEvent;
		watcher.Created += OnFsEvent;
		watcher.Deleted += OnFsEvent;
		watcher.Renamed += (_, e) => MarkPending(e.FullPath);
		watcher.Error += (_, _) => MarkPending(null);

		watcher.EnableRaisingEvents = true;
		return watcher;
	}

	private static void OnFsEvent(object sender, FileSystemEventArgs e) => MarkPending(e.FullPath);

	private static void MarkPending(string fullPath)
	{
		long now = Stopwatch.GetTimestamp();
		lock (StateLock)
		{
			if (now < _suppressEventsUntilTimestamp)
			{
				return;
			}
		}

		if (string.IsNullOrEmpty(fullPath))
		{
			lock (StateLock)
			{
				_pending = true;
				_lastEventTimestamp = now;
			}
			return;
		}

		string fileName = Path.GetFileName(fullPath);
		if (string.IsNullOrEmpty(fileName))
		{
			return;
		}

		if (fileName.StartsWith(".", StringComparison.Ordinal) ||
			fileName.EndsWith("~", StringComparison.Ordinal) ||
			fileName.Equals(".DS_Store", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		string extension = Path.GetExtension(fullPath);
		if (extension.Equals(".tmp", StringComparison.OrdinalIgnoreCase) ||
			extension.Equals(".swp", StringComparison.OrdinalIgnoreCase) ||
			extension.Equals(".swx", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}

		lock (StateLock)
		{
			_pending = true;
			_lastPath = fullPath;
			_lastEventTimestamp = now;
		}

		if (IsVerbose())
		{
			UnityEngine.Debug.Log($"[AssetRefresh] Marked pending: {fullPath}");
		}
	}

	private static void OnEditorUpdate()
	{
		TryFlushPending(force: false);
	}

	private static void OnFocusChanged(bool hasFocus)
	{
		if (hasFocus)
		{
			TryFlushPending(force: true);
		}
	}

	private static void TryFlushPending(bool force)
	{
		if (EditorApplication.isCompiling || EditorApplication.isUpdating)
		{
			return;
		}

		if (EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return;
		}

		long now = Stopwatch.GetTimestamp();
		string path;
		lock (StateLock)
		{
			if (!_pending)
			{
				return;
			}

			if (!force && SecondsSince(_lastEventTimestamp, now) < DebounceSeconds)
			{
				return;
			}

			_pending = false;
			path = _lastPath;
			_lastPath = null;
			_suppressEventsUntilTimestamp = now + SecondsToTimestampDelta(SuppressSecondsAfterRefresh);
		}

		string unityPath = ToUnityPath(path);
		if (!string.IsNullOrEmpty(unityPath))
		{
			if (unityPath.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
			{
				string maybeAssetPath = unityPath.Substring(0, unityPath.Length - ".meta".Length);
				if (!string.IsNullOrEmpty(maybeAssetPath))
				{
					AssetDatabase.ImportAsset(maybeAssetPath, ImportAssetOptions.ForceUpdate);
					if (IsVerbose())
					{
						UnityEngine.Debug.Log($"[AssetRefresh] ImportAsset: {maybeAssetPath}");
					}
					return;
				}
			}

			AssetDatabase.ImportAsset(unityPath, ImportAssetOptions.ForceUpdate);
			if (IsVerbose())
			{
				UnityEngine.Debug.Log($"[AssetRefresh] ImportAsset: {unityPath}");
			}
			return;
		}

		AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
		if (IsVerbose())
		{
			UnityEngine.Debug.Log("[AssetRefresh] Refresh()");
		}
	}

	private static double SecondsSince(long thenTimestamp, long nowTimestamp)
	{
		long delta = nowTimestamp - thenTimestamp;
		return delta <= 0 ? 0 : delta / (double)Stopwatch.Frequency;
	}

	private static long SecondsToTimestampDelta(double seconds)
	{
		if (seconds <= 0)
		{
			return 0;
		}

		double ticks = seconds * Stopwatch.Frequency;
		if (ticks >= long.MaxValue)
		{
			return long.MaxValue;
		}

		return (long)ticks;
	}

	private static string ToUnityPath(string fullPath)
	{
		if (string.IsNullOrEmpty(fullPath))
		{
			return null;
		}

		try
		{
			fullPath = Path.GetFullPath(fullPath);
		}
		catch
		{
			return null;
		}

		if (fullPath.StartsWith(AssetsRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
		{
			return "Assets" + fullPath.Substring(AssetsRoot.Length).Replace('\\', '/');
		}

		if (fullPath.StartsWith(PackagesRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
		{
			return "Packages" + fullPath.Substring(PackagesRoot.Length).Replace('\\', '/');
		}

		return null;
	}

	private static void Dispose()
	{
		lock (InitLock)
		{
			if (_assetsWatcher == null && _packagesWatcher == null)
			{
				return;
			}

			EditorApplication.update -= OnEditorUpdate;
			EditorApplication.focusChanged -= OnFocusChanged;
			AssemblyReloadEvents.beforeAssemblyReload -= Dispose;
			EditorApplication.quitting -= Dispose;

			try { _assetsWatcher?.Dispose(); } catch { }
			try { _packagesWatcher?.Dispose(); } catch { }

			_assetsWatcher = null;
			_packagesWatcher = null;

			_pending = false;
			_lastPath = null;
			_lastEventTimestamp = 0;
			_suppressEventsUntilTimestamp = 0;
		}
	}
}
#endif
