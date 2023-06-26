using System;
using System.Numerics;

[Serializable]
public class Matrix
{
	public float a = 1;
	public float b;
	public float c;
	public float d = 1;
	public float tx;
	public float ty;

	public Matrix3x2 ToMatrix3x2()
	{
		return new Matrix3x2(this.a, this.b, this.c, this.d, this.tx, this.ty);
	}

	public override string ToString()
	{
		return $"{nameof(Matrix)} a={this.a}, b={this.b}, c={this.c}, d={this.d}, tx={this.tx}, ty={this.ty}";
	}
}