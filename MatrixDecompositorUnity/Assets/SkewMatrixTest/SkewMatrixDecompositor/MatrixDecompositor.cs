using System;
using System.Numerics;
using JetBrains.Annotations;
using UnityEngine;

namespace SkewMatrixDecompositor
{
	/// <summary>
	/// This class solves the problem of decomposition of an arbitrary matrix into a series of
	/// primitive matrices, which are rotations, scaling and translation. Solves the problem of
	/// the lack of mechanisms for working with the skew-component of the matrix, for example, in Unity.
	/// </summary>
	public static class MatrixDecompositor
	{
		/// <summary>
		/// Size of probe rectangle.
		/// </summary>
		private const float RectSize = 100f;
		/// <summary>
		/// Minimal ignoring float changing.
		/// 0.00001f best value for total error less than 0.3f.
		/// </summary>
		private const float EPS = 0.00001f;
		/// <summary>
		/// Minimal ignoring rotate changing.
		/// 0.0003f best value for total error less than 0.3f.
		/// </summary>
		private const float RotateEPS = 0.0003f;
		/// <summary>
		/// Pi / 2
		/// </summary>
		private const float piDiv2 = Mathf.PI / 2f;

		/// <summary>
		/// Decomposes any matrix with skew to list of simple matrixes (rotation, scale, tranlate).
		/// </summary>
		[CanBeNull]
		public static Result Decompose(Matrix3x2 matrix)
		{
			Result result = new Result();

			if (Mathf.Abs(matrix.M31) > EPS || Mathf.Abs(matrix.M32) > EPS)
			{
				result.Translate = new UnityEngine.Vector2(matrix.M31, matrix.M32);
			}

			if (Mathf.Abs(matrix.M12) < EPS && Mathf.Abs(matrix.M21) < EPS)
			{
				// very simple matrix without rotation and skew
				if (Mathf.Abs(matrix.M11 - 1f) > EPS || Mathf.Abs(matrix.M22 - 1f) > EPS)
				{
					result.Scale2 = new UnityEngine.Vector2(matrix.M11, matrix.M22);
				}

				return result;
			}

			Rect r = new Rect();
			r.V1 = new System.Numerics.Vector2(0, 0);
			r.V2 = new System.Numerics.Vector2(RectSize, 0);
			r.V3 = new System.Numerics.Vector2(RectSize, RectSize);
			r.V4 = new System.Numerics.Vector2(0, RectSize);

			MatrixChain need = new MatrixChain(r);
			need = need.Transform(matrix);

			Rect needRect = need.Rect;

			MatrixChain now = new MatrixChain(r);

			// check angle

			float bad = need.Rect.BAD;
			if (Mathf.Abs(bad - piDiv2) < RotateEPS)
			{
				// rectangle

				float sx = Mathf.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
				float sy = Mathf.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22);
				if (Mathf.Abs(sx - 1f) > EPS || Mathf.Abs(sy - 1f) > EPS)
				{
					result.Scale2 = new UnityEngine.Vector2(sx, sy);
				}

				float a = MathF.Atan2(-matrix.M21, matrix.M22);
				if (Mathf.Abs(a) > EPS)
				{
					result.Rotate2 = a;
				}

				return result;
			}

			bool pointsChanged = false;
			if (Mathf.Abs(bad) > piDiv2)
			{
				pointsChanged = true;
				need.UseRect2 = true;
				now.UseRect2 = true;

				needRect = need.Rect;
			}

			float ah = needRect.AH;
			float ac = needRect.AC;
			float bh = needRect.BH;
			float angleBAXNeed = Mathf.Atan2(needRect.C.Y - needRect.A.Y, needRect.C.X - needRect.A.X);

			// scale AB / AD

			float k = ah / ac;
			float abb = bh / Mathf.Sqrt(1 - Mathf.Abs(k));

			float bbcca = Mathf.Acos(bh / abb);
			if (float.IsNaN(bbcca)) return null;

			float add = bh / Mathf.Sin(bbcca);
			float ABScale = abb / now.Rect.AB;
			float ADScale = add / now.Rect.AD;

			if (pointsChanged)
			{
				(ABScale, ADScale) = (ADScale, ABScale);
			}

			if (Mathf.Abs(ADScale - 1f) > EPS || Mathf.Abs(ABScale - 1f) > EPS)
			{
				result.Scale1 = new UnityEngine.Vector2(ADScale, ABScale);
				Matrix3x2 scaleMatrix = Matrix3x2.CreateScale(ADScale, ABScale);
				now = now.Transform(scaleMatrix);
			}

			// rotate on diagonal

			float rot = pointsChanged ? piDiv2 - now.Rect.CAD : -now.Rect.CAD;
			if (Mathf.Abs(rot) > EPS)
			{
				result.Rotate1 = rot;
				Matrix3x2 rotMatrix = Matrix3x2.CreateRotation(rot);
				now = now.Transform(rotMatrix);
			}

			// skew

			float ACScale = ac / now.Rect.AC;
			float BHScale = bh / now.Rect.BH;

			if (Mathf.Abs(ACScale - 1f) > EPS || Mathf.Abs(BHScale - 1f) > EPS)
			{
				result.Scale2 = new UnityEngine.Vector2(ACScale, BHScale);
			}

			// rotate
			var ang = ABScale < 0 ? Mathf.PI + angleBAXNeed : angleBAXNeed;
			if (Mathf.Abs(ang) > EPS)
			{
				result.Rotate2 = ang;
			}

			return result;
		}

		public class Result
		{
			public UnityEngine.Vector2? Scale1;
			public float? Rotate1;
			public UnityEngine.Vector2? Scale2;
			public float? Rotate2;
			public UnityEngine.Vector2? Translate;
		}
	}
}