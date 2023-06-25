using System.Numerics;

namespace MatrixDecompositorCore;

/// <summary>
/// This class solves the problem of decomposition of an arbitrary matrix into a series of
/// primitive matrices, which are rotations, scaling and translation. Solves the problem of
/// the lack of mechanisms for working with the skew-component of the matrix, for example, in Unity.
/// </summary>
public class MatrixDecompositor
{
	private const float RectSize = 100f;
	private const float EPS = 0.00001f;
	private const float piDiv2 = MathF.PI / 2f;

	/// <summary>
	/// Decomposes any matrix with skew to list of simple matrixes (rotation, scale, tranlate).
	/// </summary>
	public static List<Matrix3x2>? Decompose(Matrix3x2 matrix)
	{
		List<Matrix3x2> result = new List<Matrix3x2>();

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
		bool pointsChanged = false;
		if (MathF.Abs(need.Rect.BAD) > piDiv2)
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
			Matrix3x2 scaleMatrix = Matrix3x2.CreateScale(ADScale, ABScale);
			result.Add(scaleMatrix);
			now = now.Transform(scaleMatrix);
		}

		// rotate on diagonal

		float rot;
		if (pointsChanged)
		{
			rot = piDiv2 - now.Rect.CAD;
		}
		else
		{
			rot = -now.Rect.CAD;
		}

		if (MathF.Abs(rot) > EPS)
		{
			Matrix3x2 rotMatrix = Matrix3x2.CreateRotation(rot);
			result.Add(rotMatrix);
			now = now.Transform(rotMatrix);
		}

		// skew

		float ACScale = ac / now.Rect.AC;
		float BHScale = bh / now.Rect.BH;

		if (MathF.Abs(ACScale - 1f) > EPS || MathF.Abs(BHScale - 1f) > EPS)
		{
			Matrix3x2 skewMatrix = Matrix3x2.CreateScale(ACScale, BHScale);
			result.Add(skewMatrix);
		}

		// rotate
		var ang = ABScale < 0 ? MathF.PI + angleBAXNeed : angleBAXNeed;
		if (MathF.Abs(ang) > EPS)
		{
			Matrix3x2 rm = Matrix3x2.CreateRotation(ang);
			result.Add(rm);
		}

		//translate
		if (MathF.Abs(need.FullMatrix.M31) > EPS || MathF.Abs(need.FullMatrix.M32) > EPS)
		{
			Matrix3x2 tm = Matrix3x2.CreateTranslation(need.FullMatrix.M31, need.FullMatrix.M32);
			result.Add(tm);
		}

		return result;
	}
}