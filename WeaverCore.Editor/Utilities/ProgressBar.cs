using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Utilities
{
	public sealed class ProgressBar : IDisposable
	{
		bool disposed = false;

		bool _enabled_ = true;
		public bool Enabled
		{
			get
			{
				return _enabled_;
			}
			set
			{
				if (_enabled_ != value)
				{
					_enabled_ = value;
					if (_enabled_)
					{
						EditorApplication.update += OnUpdate;
					}
					else
					{
						EditorApplication.update -= OnUpdate;
						EditorUtility.ClearProgressBar();
					}
				}
			}
		}
		public int CurrentStep { get; set; }
		public int ProgressSteps { get; set; }
		public string Title { get; set; }
		public string Info { get; set; }

		public void GoToNextStep()
		{
			CurrentStep++;
		}

		void OnUpdate()
		{
			SetProgressBar();
		}

		public ProgressBar(int progressSteps, string title, string info, bool enabled = true)
		{
			Title = title;
			Info = info;
			EditorApplication.update += OnUpdate;
			ProgressSteps = progressSteps;
			Enabled = enabled;
		}

		void SetProgressBar()
		{
			if (!Enabled)
			{
				EditorUtility.ClearProgressBar();
				return;
			}
			if (CurrentStep > ProgressSteps)
			{
				CurrentStep = ProgressSteps;
			}
			if (ProgressSteps == 0)
			{
				EditorUtility.ClearProgressBar();
			}
			else
			{
				EditorUtility.DisplayProgressBar(Title, Info, CurrentStep / (float)ProgressSteps);
			}
		}

		public void Dispose()
		{
			if (!disposed)
			{
				disposed = true;
				if (Enabled)
				{
					EditorApplication.update -= OnUpdate;
					EditorUtility.ClearProgressBar();
				}
			}
		}

		~ProgressBar()
		{
			Dispose();
		}
	}
}
