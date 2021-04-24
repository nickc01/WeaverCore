using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Utilities
{
	public static class MathUtilties
	{
		public static Vector2 CalculateVelocityToReachPoint(Vector2 start, Vector2 end, double time, double gravityScale)
		{
			Debug.DrawLine(start, end, Color.blue, 5f);

			//time /= 2.0;

			double a = Physics2D.gravity.y * gravityScale;

			double yDest = end.y - start.y;

			double yVelocity = (yDest / time) - (0.5 * a * time);
			//float xVelocity = Mathf.Lerp(start.x,end.x,time);
			double xVelocity = (end.x - (double)start.x) / time;

			return new Vector2((float)xVelocity, (float)yVelocity);
			//time /= 2f;
			//return new Vector2((end.x - start.x) / (time * 2f), CalculateVerticalVelocity(start.y,end.y,time,gravityScale));
		}

		static float CalculateVerticalVelocity(float startY, float endY, float time, float gravityScale)
		{
			float a = Physics2D.gravity.y * gravityScale;
			float newY = endY - startY;

			return (newY / time) - (a * time);
		}

		//Returns the time to get to the peak (x value), and the peak (y value)
		public static Vector2 CalculateMaximumOfCurve(float startY, float endY, float time, float gravityScale)
		{
			var velocity = CalculateVerticalVelocity(startY, endY, time, gravityScale);

			var a = gravityScale * Physics2D.gravity.y;

			var timeToPeak = -velocity / (2f * a);

			var peakValue = (a * timeToPeak * timeToPeak) + (velocity * timeToPeak);

			return new Vector2(timeToPeak, peakValue);
		}

		public static Vector2 PolarToCartesian(float angleDegrees,float magnitude)
		{
			return new Vector2(Mathf.Cos(Mathf.Deg2Rad * angleDegrees) * magnitude,Mathf.Sin(Mathf.Deg2Rad * angleDegrees) * magnitude);
		}

		public static Vector2 PolarToCartesian(Vector2 angleAndMagnitude)
		{
			return PolarToCartesian(angleAndMagnitude.x, angleAndMagnitude.y);
		}

		public static Vector2 CartesianToPolar(Vector2 vector)
		{
			return CartesianToPolar(vector.x, vector.y);
		}

		public static Vector2 CartesianToPolar(float x, float y)
		{
			return new Vector2(Mathf.Atan2(y,x) * Mathf.Rad2Deg,Mathf.Sqrt((x*x) + (y*y)));
		}

		/*public static Vector2 CalculateVelocityToReachPointNEW(Vector2 start, Vector2 end, double time, double gravityScale)
		{
			Debug.DrawLine(start, end, Color.red,5f);

			//time /= 2.0;

			double a = Physics2D.gravity.y * gravityScale;

			double yDest = end.y - start.y;

			double yVelocity = (yDest / time) - (0.5 * a * time);
			//float xVelocity = Mathf.Lerp(start.x,end.x,time);
			double xVelocity = (end.x - (double)start.x) / time;

			return new Vector2((float)xVelocity,(float)yVelocity);
		}*/
	}
}
