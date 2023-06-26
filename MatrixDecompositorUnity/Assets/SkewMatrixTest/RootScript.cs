using SkewMatrixDecompositor;
using UnityEngine;
using Matrix3x2 = System.Numerics.Matrix3x2;

public class RootScript : MonoBehaviour
{
	public Matrix Matrix = new Matrix();
	public GameObject Target;

	private MatrixObjectWrapper wrapper;

	void Start()
	{
		if (this.Target != null)
		{
			Matrix3x2 matrix = this.Matrix.ToMatrix3x2();
			this.wrapper = new MatrixObjectWrapper(this.Target, matrix);
		}
	}

	void Update()
	{
		Matrix3x2 matrix = this.Matrix.ToMatrix3x2();

		if (this.wrapper != null)
		{
			this.wrapper.Update(matrix);
		}
	}
}