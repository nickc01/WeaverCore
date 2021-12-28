using System.Collections;
using TMPro;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Displays a "Map Updated" icon in the bottom left corner. Used when the player sits on a <see cref="WeaverBench"/>
	/// </summary>
	public class MapUpdateDisplay : MonoBehaviour
	{
		[SerializeField]
		[Tooltip("Should the Icon be displayed immediately upon spawning in?")]
		bool autoUp = false;
		[SerializeField]
		[Tooltip("The delay before the icon fades in")]
		float upDelay = 0.5f;
		[SerializeField]
		[Tooltip("The time it takes for the icon to fade in")]
		float upTime = 0.25f;
		[SerializeField]
		Color downColor = new Color(1f, 1f, 1f, 0f);
		[SerializeField]
		Color newColor = new Color(0f,0f,0f,1f);
		[SerializeField]
		Color prevColor = new Color(1f, 1f, 1f, 1f);
		[SerializeField]
		Color startColor = new Color(1f, 1f, 1f, 1f);
		[SerializeField]
		Color upColor = new Color(1f, 1f, 1f, 1f);

		SpriteRenderer sRenderer;
		TMP_Text tmp;

		Color MeshColor
		{
			get
			{
				if (sRenderer != null)
				{
					return sRenderer.color;
				}
				else if (tmp != null)
				{
					return tmp.color;
				}
				return default;
			}

			set
			{
				if (sRenderer != null)
				{
					sRenderer.color = value;
				}
				else if (tmp != null)
				{
					tmp.color = value;
				}
			}
		}

		private void Awake()
		{
			sRenderer = GetComponent<SpriteRenderer>();
			tmp = GetComponent<TMP_Text>();
			if (sRenderer != null)
			{
				sRenderer.color = startColor;
			}

			if (tmp != null)
			{
				tmp.color = startColor;
			}

			if (autoUp)
			{
				StartCoroutine(DelayRoutine(upDelay,UpRoutine()));
			}
			else
			{
				if (sRenderer != null)
				{
					sRenderer.enabled = false;
				}
				if (tmp != null)
				{
					tmp.enabled = false;
				}
			}
		}

		IEnumerator DelayRoutine(float time, IEnumerator next)
		{
			yield return new WaitForSeconds(time);
			yield return next;
		}

		public void Up()
		{
			StopAllCoroutines();
			StartCoroutine(UpRoutine());
		}

		IEnumerator UpRoutine()
		{
			prevColor = MeshColor;
			if (sRenderer != null)
			{
				sRenderer.enabled = true;
			}
			if (tmp != null)
			{
				tmp.enabled = true;
			}
			newColor = prevColor;
			for (float i = 0; i < upTime; i += Time.deltaTime)
			{
				newColor = Color.Lerp(prevColor, upColor, i / upTime);
				MeshColor = newColor;

				if (tmp != null)
				{
					tmp.color = MeshColor;
				}
				yield return null;
			}


			newColor = upColor;
			MeshColor = newColor;

			if (tmp != null)
			{
				tmp.color = MeshColor;
			}

			yield break;
		}

		public void Down()
		{
			StopAllCoroutines();
			StartCoroutine(DownRoutine());
		}

		IEnumerator DownRoutine()
		{
			prevColor = MeshColor;
			if (sRenderer != null)
			{
				sRenderer.enabled = true;
			}
			if (tmp != null)
			{
				tmp.enabled = true;
			}
			newColor = prevColor;
			for (float i = 0; i < upTime; i += Time.deltaTime)
			{
				newColor = Color.Lerp(prevColor, downColor, i / upTime);
				MeshColor = newColor;

				if (tmp != null)
				{
					tmp.color = MeshColor;
				}
				yield return null;
			}

			newColor = downColor;
			MeshColor = newColor;

			if (tmp != null)
			{
				tmp.color = MeshColor;
			}

			CheckDownAlpha();

			yield break;
		}

		private void CheckDownAlpha()
		{
			if (downColor.a == 0f)
			{
				if (sRenderer != null)
				{
					sRenderer.enabled = false;
				}
				if (tmp != null)
				{
					tmp.enabled = false;
				}
			}
		}

		public void UpInstant()
		{
			if (sRenderer != null)
			{
				sRenderer.enabled = true;
				sRenderer.color = upColor;
			}
			if (tmp != null)
			{
				tmp.enabled = true;
				tmp.color = upColor;
			}
		}

		public void PulseDown()
		{
			UpInstant();
			Down();
		}

		public void FadeUpInstant()
		{
			DownInstant();
			Up();
		}

		public void DownInstant()
		{
			if (sRenderer != null)
			{
				sRenderer.enabled = true;
				sRenderer.color = downColor;
			}
			if (tmp != null)
			{
				tmp.enabled = true;
				tmp.color = downColor;
			}
			CheckDownAlpha();
		}
	}
}
