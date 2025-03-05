using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMPro
{
	[DisallowMultipleComponent]
	public class TMP_SpriteAnimator : MonoBehaviour
	{
		private Dictionary<int, bool> m_animations = new Dictionary<int, bool>(16);

		private TMP_Text m_TextComponent;

		private void Awake()
		{
			m_TextComponent = GetComponent<TMP_Text>();
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		public void StopAllAnimations()
		{
			StopAllCoroutines();
			m_animations.Clear();
		}

		public void DoSpriteAnimation(int currentCharacter, TMP_SpriteAsset spriteAsset, int start, int end, int framerate)
		{
			bool value = false;
			if (!m_animations.TryGetValue(currentCharacter, out value))
			{
				StartCoroutine(DoSpriteAnimationInternal(currentCharacter, spriteAsset, start, end, framerate));
				m_animations.Add(currentCharacter, true);
			}
		}

		private IEnumerator DoSpriteAnimationInternal(int currentCharacter, TMP_SpriteAsset spriteAsset, int start, int end, int framerate)
		{
			if ((Object)(object)m_TextComponent == null)
			{
				yield break;
			}
			yield return null;
			int currentFrame = start;
			if (end > spriteAsset.spriteInfoList.Count)
			{
				end = spriteAsset.spriteInfoList.Count - 1;
			}
			TMP_CharacterInfo charInfo = m_TextComponent.textInfo.characterInfo[currentCharacter];
			int materialIndex = charInfo.materialReferenceIndex;
			int vertexIndex = charInfo.vertexIndex;
			TMP_MeshInfo meshInfo = m_TextComponent.textInfo.meshInfo[materialIndex];
			float elapsedTime = 0f;
			float targetTime = 1f / (float)Mathf.Abs(framerate);
			while (true)
			{
				if (elapsedTime > targetTime)
				{
					elapsedTime = 0f;
					TMP_Sprite sprite = spriteAsset.spriteInfoList[currentFrame];
					Vector3[] vertices = meshInfo.vertices;
					Vector2 origin = new Vector2(charInfo.origin, charInfo.baseLine);
					float spriteScale = charInfo.fontAsset.fontInfo.Ascender / sprite.height * sprite.scale * charInfo.scale;
					Vector3 bl = new Vector3(origin.x + sprite.xOffset * spriteScale, origin.y + (sprite.yOffset - sprite.height) * spriteScale);
					Vector3 tl = new Vector3(bl.x, origin.y + sprite.yOffset * spriteScale);
					Vector3 tr = new Vector3(origin.x + (sprite.xOffset + sprite.width) * spriteScale, tl.y);
					Vector3 br = new Vector3(tr.x, bl.y);
					vertices[vertexIndex] = bl;
					vertices[vertexIndex + 1] = tl;
					vertices[vertexIndex + 2] = tr;
					vertices[vertexIndex + 3] = br;
					Vector2[] uvs0 = meshInfo.uvs0;
					Vector2 uv0 = new Vector2(sprite.x / (float)spriteAsset.spriteSheet.width, sprite.y / (float)spriteAsset.spriteSheet.height);
					Vector2 uv1 = new Vector2(uv0.x, (sprite.y + sprite.height) / (float)spriteAsset.spriteSheet.height);
					Vector2 uv2 = new Vector2((sprite.x + sprite.width) / (float)spriteAsset.spriteSheet.width, uv1.y);
					Vector2 uv3 = new Vector2(uv2.x, uv0.y);
					uvs0[vertexIndex] = uv0;
					uvs0[vertexIndex + 1] = uv1;
					uvs0[vertexIndex + 2] = uv2;
					uvs0[vertexIndex + 3] = uv3;
					meshInfo.mesh.vertices = vertices;
					meshInfo.mesh.uv = uvs0;
					m_TextComponent.UpdateGeometry(meshInfo.mesh, materialIndex);
					currentFrame = ((framerate > 0) ? ((currentFrame >= end) ? start : (currentFrame + 1)) : ((currentFrame <= start) ? end : (currentFrame - 1)));
				}
				elapsedTime += Time.deltaTime;
				yield return null;
			}
		}
	}
}
