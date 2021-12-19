﻿using System;
using System.Collections;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;
using PackageClient = UnityEditor.PackageManager.Client;

namespace WeaverCore.Editor
{

    class VerifyPackages : DependencyCheck
    {

        static Action<DependencyCheckResult> Finish;

		static IEnumerator WaitForRequest<T>(Request<T> request) => new WaitUntil(() => request.IsCompleted);

		public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            Finish = finishCheck;
			UnboundCoroutine.Start(PackageCheckRoutine());
        }

        static IEnumerator PackageCheckRoutine()
        {
			var listRequest = PackageClient.List();
			var searchRequest = PackageClient.Search("com.unity.scriptablebuildpipeline");
			yield return WaitForRequest(listRequest);
			if (listRequest.Status == StatusCode.Failure)
			{
				Debug.LogError($"Failed to get a package listing with error code [{listRequest.Error.errorCode}], {listRequest.Error.message}");
			}
			yield return WaitForRequest(searchRequest);
			if (searchRequest.Status == StatusCode.Failure)
			{
				Debug.LogError($"Failed to do a package search with the error code [{searchRequest.Error.errorCode}], {searchRequest.Error.message}");
			}

			var buildPipelineLatestVersion = searchRequest.Result[0].versions.latestCompatible;

			bool latestVersionInstalled = false;

			foreach (var package in listRequest.Result)
			{
				if (package.name == "com.unity.textmeshpro")
				{
					DebugUtilities.ClearLog();
					Debug.Log("Removing Text Mesh Pro package, since WeaverCore provides a version that is compatible with Hollow Knight");
					//makingChanges = true;
					PackageClient.Remove(package.name);

					Finish(DependencyCheckResult.RequiresReload);
					yield break;
					//DebugUtilities.ClearLog();
					//Break - since we can only do one request at a time
					//break;
				}
				else if (package.name == "com.unity.scriptablebuildpipeline")
				{
					//If it isn't the latest compatible version
					if (package.version != buildPipelineLatestVersion)
					{
						DebugUtilities.ClearLog();
						Debug.Log($"Updating the Scriptable Build Pipeline from [{package.version}] -> [{buildPipelineLatestVersion}]");
						PackageClient.Remove(package.name);
						//makingChanges = true;
						//DebugUtilities.ClearLog();
						//Break - since we can only do one request at a time
						//break;
						Finish(DependencyCheckResult.RequiresReload);
						yield break;
					}
					else
					{
						latestVersionInstalled = true;
					}

				}
			}

			if (!latestVersionInstalled)
			{
				DebugUtilities.ClearLog();
				PackageClient.Add("com.unity.scriptablebuildpipeline@" + buildPipelineLatestVersion);

				Finish(DependencyCheckResult.RequiresReload);
				yield break;
			}

			Finish(DependencyCheckResult.Complete);
		}
    }
}
