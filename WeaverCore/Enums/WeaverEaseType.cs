using UnityEngine;

namespace WeaverCore.Enums
{
	public enum WeaverEaseType
	{
		easeInQuad = 0,
		easeOutQuad = 1,
		easeInOutQuad = 2,
		easeInCubic = 3,
		easeOutCubic = 4,
		easeInOutCubic = 5,
		easeInQuart = 6,
		easeOutQuart = 7,
		easeInOutQuart = 8,
		easeInQuint = 9,
		easeOutQuint = 10,
		easeInOutQuint = 11,
		easeInSine = 12,
		easeOutSine = 13,
		easeInOutSine = 14,
		easeInExpo = 15,
		easeOutExpo = 16,
		easeInOutExpo = 17,
		easeInCirc = 18,
		easeOutCirc = 19,
		easeInOutCirc = 20,
		linear = 21,
		spring = 22,
		bounce = 23,
		easeInBack = 24,
		easeOutBack = 25,
		easeInOutBack = 26,
		elastic = 27,
		punch = 28
	}

	/// <summary>
	/// WeaverEaseType extensions.
	/// </summary>
	public static class WeaverEaseExtensions
	{
		/// <summary>
		/// Builds an AnimationCurve that evaluates the chosen easing function over t in [0,1].
		/// For spring/elastic/back/bounce the curve may overshoot; punch is zero-centered.
		/// </summary>
		public static AnimationCurve ToAnimationCurve(this WeaverEaseType easeType)
		{
			const int samples = 64; // sufficient density for smooth playback
			var tVals = new float[samples];
			var yVals = new float[samples];

			for (int i = 0; i < samples; i++)
			{
				float t = i / (samples - 1f);
				tVals[i] = t;
				yVals[i] = Evaluate(easeType, t);
			}

			// Create keys with linear-ish tangents to avoid Bezier overshoot artifacts
			var keys = new Keyframe[samples];
			for (int i = 0; i < samples; i++)
			{
				float t = tVals[i];
				float y = yVals[i];

				float tanIn, tanOut;

				if (i == 0)
				{
					// forward difference
					float dy = yVals[i + 1] - yVals[i];
					float dt = tVals[i + 1] - tVals[i];
					tanIn = tanOut = dy / dt;
				}
				else if (i == samples - 1)
				{
					// backward difference
					float dy = yVals[i] - yVals[i - 1];
					float dt = tVals[i] - tVals[i - 1];
					tanIn = tanOut = dy / dt;
				}
				else
				{
					// central difference
					float dy = yVals[i + 1] - yVals[i - 1];
					float dt = tVals[i + 1] - tVals[i - 1];
					float tan = dy / dt;
					tanIn = tanOut = tan;
				}

				keys[i] = new Keyframe(t, y, tanIn, tanOut);
			}

			var curve = new AnimationCurve(keys)
			{
				preWrapMode = WrapMode.ClampForever,
				postWrapMode = WrapMode.ClampForever
			};
			return curve;
		}

		/// <summary>Evaluate easing (0..1 -> value). Mirrors PlayMaker formulas with start=0, end=1.</summary>
		static float Evaluate(WeaverEaseType type, float t)
		{
			switch (type)
			{
				case WeaverEaseType.linear: return t;

				// Quad
				case WeaverEaseType.easeInQuad: return t * t;
				case WeaverEaseType.easeOutQuad: return t * (2f - t);
				case WeaverEaseType.easeInOutQuad:
					return t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

				// Cubic
				case WeaverEaseType.easeInCubic: return t * t * t;
				case WeaverEaseType.easeOutCubic: { float u = t - 1f; return u * u * u + 1f; }
				case WeaverEaseType.easeInOutCubic:
					return t < 0.5f ? 4f * t * t * t : 1f + 4f * (t - 1f) * (t - 1f) * (t - 1f);

				// Quart
				case WeaverEaseType.easeInQuart: return t * t * t * t;
				case WeaverEaseType.easeOutQuart: { float u = t - 1f; return 1f - (u * u * u * u); }
				case WeaverEaseType.easeInOutQuart:
					if (t < 0.5f) return 8f * t * t * t * t;
					{ float u = t - 1f; return 1f - 8f * u * u * u * u; }

				// Quint
				case WeaverEaseType.easeInQuint: return t * t * t * t * t;
				case WeaverEaseType.easeOutQuint: { float u = t - 1f; return u * u * u * u * u + 1f; }
				case WeaverEaseType.easeInOutQuint:
					if (t < 0.5f) return 16f * t * t * t * t * t;
					{ float u = (t - 1f); return 1f + 16f * u * u * u * u * u; }

				// Sine
				case WeaverEaseType.easeInSine: return 1f - Mathf.Cos(t * (Mathf.PI * 0.5f));
				case WeaverEaseType.easeOutSine: return Mathf.Sin(t * (Mathf.PI * 0.5f));
				case WeaverEaseType.easeInOutSine: return 0.5f * (1f - Mathf.Cos(Mathf.PI * t));

				// Expo
				case WeaverEaseType.easeInExpo:
					return Mathf.Approximately(t, 0f) ? 0f : Mathf.Pow(2f, 10f * (t - 1f));
				case WeaverEaseType.easeOutExpo:
					return Mathf.Approximately(t, 1f) ? 1f : 1f - Mathf.Pow(2f, -10f * t);
				case WeaverEaseType.easeInOutExpo:
					if (Mathf.Approximately(t, 0f)) return 0f;
					if (Mathf.Approximately(t, 1f)) return 1f;
					if (t < 0.5f) return 0.5f * Mathf.Pow(2f, 10f * (2f * t - 1f));
					return 1f - 0.5f * Mathf.Pow(2f, -10f * (2f * t - 1f));

				// Circ
				case WeaverEaseType.easeInCirc: return 1f - Mathf.Sqrt(1f - t * t);
				case WeaverEaseType.easeOutCirc: { float u = t - 1f; return Mathf.Sqrt(1f - u * u); }
				case WeaverEaseType.easeInOutCirc:
					if (t < 0.5f)
					{
						float u = 2f * t;
						return 0.5f * (1f - Mathf.Sqrt(1f - u * u));
					}
					else
					{
						float u = 2f * t - 2f;
						return 0.5f * (Mathf.Sqrt(1f - u * u) + 1f);
					}

				// Back
				case WeaverEaseType.easeInBack:
				{
					const float s = 1.70158f;
					return t * t * ((s + 1f) * t - s);
				}
				case WeaverEaseType.easeOutBack:
				{
					const float s = 1.70158f;
					float u = t - 1f;
					return u * u * ((s + 1f) * u + s) + 1f;
				}
				case WeaverEaseType.easeInOutBack:
				{
					float s = 1.70158f * 1.525f;
					if (t < 0.5f)
					{
						float u = t * 2f;
						return 0.5f * (u * u * ((s + 1f) * u - s));
					}
					else
					{
						float u = t * 2f - 2f;
						return 0.5f * (u * u * ((s + 1f) * u + s) + 2f);
					}
				}

				// Spring (PlayMaker-style)
				case WeaverEaseType.spring:
				{
					float v = Mathf.Clamp01(t);
					v = (Mathf.Sin(v * Mathf.PI * (0.2f + 2.5f * v * v * v)) * Mathf.Pow(1f - v, 2.2f) + v) * (1f + 1.2f * (1f - v));
					return v;
				}

				// Bounce (out-bounce)
				case WeaverEaseType.bounce:
				{
					float v = t;
					if (v < 0.36363637f) return 7.5625f * v * v;
					if (v < 0.72727275f) { v -= 0.54545456f; return 7.5625f * v * v + 0.75f; }
					if (v < 0.90909094f) { v -= 0.8181818f; return 7.5625f * v * v + 0.9375f; }
					v -= 21f / 22f; return 7.5625f * v * v + 63f / 64f;
				}

				// Elastic (out-elastic)
				case WeaverEaseType.elastic:
				{
					if (Mathf.Approximately(t, 0f)) return 0f;
					if (Mathf.Approximately(t, 1f)) return 1f;
					const float p = 0.3f; // period
					float s = p / 4f;     // phase
					return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
				}

				// Punch (zero-centered “impulse/settle”)
				case WeaverEaseType.punch:
				{
					if (Mathf.Approximately(t, 0f) || Mathf.Approximately(t, 1f)) return 0f;
					const float amplitude = 1f;
					const float p = 0.3f;
					float s = 0f; // asin(0) == 0
					return amplitude * Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p);
				}

				default: return t;
			}
		}
	}
}
