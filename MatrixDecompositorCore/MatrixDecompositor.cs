using System.Collections.Generic;
using System.Numerics;

namespace MatrixDecompositorCore;

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
	private const float piDiv2 = MathF.PI / 2f;

	public static List<Matrix3x2>? Decompose(Matrix3x2 matrix)
	{
		Result? result = GetDecomposeResult(matrix);
		if (result == null)
		{
			return null;
		}

		List<Matrix3x2> list = new List<Matrix3x2>();
		if (result.Scale1 != null)
		{
			list.Add(Matrix3x2.CreateScale(result.Scale1.Value.X, result.Scale1.Value.Y));
		}

		if (result.Rotate1 != null)
		{
			list.Add(Matrix3x2.CreateRotation(result.Rotate1.Value));
		}

		if (result.Scale2 != null)
		{
			list.Add(Matrix3x2.CreateScale(result.Scale2.Value.X, result.Scale2.Value.Y));
		}

		if (result.Rotate2 != null)
		{
			list.Add(Matrix3x2.CreateRotation(result.Rotate2.Value));
		}

		if (result.Translate != null)
		{
			list.Add(Matrix3x2.CreateTranslation(result.Translate.Value.X, result.Translate.Value.Y));
		}

		return list;
	}

	/// <summary>
	/// Decomposes any matrix with skew to list of simple matrixes (rotation, scale, tranlate).
	/// </summary>
	public static Result? GetDecomposeResult(Matrix3x2 matrix)
	{
		Result result = new Result();

		if (MathF.Abs(matrix.M31) > EPS || MathF.Abs(matrix.M32) > EPS)
		{
			result.Translate = new Vector2(matrix.M31, matrix.M32);
		}
		if (MathF.Abs(matrix.M12) < EPS && MathF.Abs(matrix.M21) < EPS)
		{
			// very simple matrix without rotation and skew
			if (MathF.Abs(matrix.M11 - 1f) > EPS || MathF.Abs(matrix.M22 - 1f) > EPS)
			{
				result.Scale2 = new Vector2(matrix.M11, matrix.M22);
			}

			return result;
		}

		Rect r = new Rect();
		r.V1 = new Vector2(0, 0);
		r.V2 = new Vector2(RectSize, 0);
		r.V3 = new Vector2(RectSize, RectSize);
		r.V4 = new Vector2(0, RectSize);

		MatrixChain need = new MatrixChain(r);
		need = need.Transform(matrix);

		Rect needRect = need.Rect;

		MatrixChain now = new MatrixChain(r);

		// check angle

		float bad = need.Rect.BAD;
		if (MathF.Abs(bad - piDiv2) < RotateEPS)
		{
			// rectangle

			float sx = MathF.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
			float sy = MathF.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22);
			if (MathF.Abs(sx - 1f) > EPS || MathF.Abs(sy - 1f) > EPS)
			{
				result.Scale2 = new Vector2(sx, sy);
			}

			float a = MathF.Atan2(-matrix.M21, matrix.M22);
			if (MathF.Abs(a) > EPS)
			{
				result.Rotate2 = a;
			}

			return result;
		}

		bool pointsChanged = false;
		if (MathF.Abs(bad) > piDiv2)
		{
			pointsChanged = true;
			need.UseRect2 = true;
			now.UseRect2 = true;

			needRect = need.Rect;
		}

		float ah = needRect.AH;
		float ac = needRect.AC;
		float bh = needRect.BH;
		float angleBAXNeed = MathF.Atan2(needRect.C.Y - needRect.A.Y, needRect.C.X - needRect.A.X);

		// scale AB / AD

		float k = ah / ac;
		float abb = bh / MathF.Sqrt(1 - MathF.Abs(k));

		float bbcca = MathF.Acos(bh / abb);
		if (float.IsNaN(bbcca)) return null;

		float add = bh / MathF.Sin(bbcca);
		float ABScale = abb / now.Rect.AB;
		float ADScale = add / now.Rect.AD;

		if (pointsChanged)
		{
			(ABScale, ADScale) = (ADScale, ABScale);
		}

		if (MathF.Abs(ADScale - 1f) > EPS || MathF.Abs(ABScale - 1f) > EPS)
		{
			result.Scale1 = new Vector2(ADScale, ABScale);
			Matrix3x2 scaleMatrix = Matrix3x2.CreateScale(ADScale, ABScale);
			now = now.Transform(scaleMatrix);
		}

		// rotate on diagonal

		float rot = pointsChanged ? piDiv2 - now.Rect.CAD : -now.Rect.CAD;
		if (MathF.Abs(rot) > EPS)
		{
			result.Rotate1 = rot;
			Matrix3x2 rotMatrix = Matrix3x2.CreateRotation(rot);
			now = now.Transform(rotMatrix);
		}

		// skew

		float ACScale = ac / now.Rect.AC;
		float BHScale = bh / now.Rect.BH;

		if (MathF.Abs(ACScale - 1f) > EPS || MathF.Abs(BHScale - 1f) > EPS)
		{
			result.Scale2 = new Vector2(ACScale, BHScale);
		}

		// rotate
		var ang = ABScale < 0 ? MathF.PI + angleBAXNeed : angleBAXNeed;
		if (MathF.Abs(ang) > EPS)
		{
			result.Rotate2 = ang;
		}

		return result;
	}

	public class Result
	{
		public Vector2? Scale1;
		public float? Rotate1;
		public Vector2? Scale2;
		public float? Rotate2;
		public Vector2? Translate;
	}
}