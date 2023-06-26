using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

namespace SkewMatrixDecompositor
{
	internal class Rect
	{
		#region v1 - v4

		public Vector2 V1
		{
			get
			{
				return this.Vectors[0];
			}
			set
			{
				this.Vectors[0] = value;
			}
		}

		public Vector2 V2
		{
			get
			{
				return this.Vectors[1];
			}
			set
			{
				this.Vectors[1] = value;
			}
		}

		public Vector2 V3
		{
			get
			{
				return this.Vectors[2];
			}
			set
			{
				this.Vectors[2] = value;
			}
		}

		public Vector2 V4
		{
			get
			{
				return this.Vectors[3];
			}
			set
			{
				this.Vectors[3] = value;
			}
		}

		#endregion

		#region A, B, C, D

		public Vector2 A
		{
			get
			{
				return this.V1;
			}
		}

		private Vector2 B
		{
			get
			{
				return this.V4;
			}
		}

		public Vector2 C
		{
			get
			{
				return this.V3;
			}
		}

		private Vector2 D
		{
			get
			{
				return this.V2;
			}
		}

		#endregion

		#region lengths

		public float AB
		{
			get
			{
				return Dist(this.A, this.B);
			}
		}

		public float AC
		{
			get
			{
				return Dist(this.A, this.C);
			}
		}

		public float AD
		{
			get
			{
				return Dist(this.A, this.D);
			}
		}

		private static float Hyp(float a, float b)
		{
			return Mathf.Sqrt(a * a + b * b);
		}

		private static float Dist(Vector2 v1, Vector2 v2)
		{
			return Hyp(v2.X - v1.X, v2.Y - v1.Y);
		}

		#endregion

		#region angles

		private float BAC
		{
			get
			{
				return Angle(this.A, this.B, this.C);
			}
		}

		public float CAD
		{
			get
			{
				return Angle(this.A, this.C, this.D);
			}
		}

		public float BAD
		{
			get
			{
				return Angle(this.A, this.B, this.D);
			}
		}

		/// <summary>
		/// Get angle between line (v0, v1) and line (v0, v2).
		/// </summary>
		private static float Angle(Vector2 v0, Vector2 v1, Vector2 v2)
		{
			float angle1 = Mathf.Atan2(v1.Y - v0.Y, v1.X - v0.X);
			float angle2 = Mathf.Atan2(v2.Y - v0.Y, v2.X - v0.X);
			float radians = angle1 - angle2;
			while (radians > Mathf.PI) radians -= Mathf.PI * 2;
			while (radians < -Mathf.PI) radians += Mathf.PI * 2;
			return radians;
		}

		#endregion

		public float AH => this.AB * Mathf.Cos(this.BAC);
		public float BH => this.AB * Mathf.Sin(this.BAC);

		private readonly List<Vector2> Vectors = new List<Vector2>(4);

		public Rect()
		{
			this.Vectors.Add(new Vector2());
			this.Vectors.Add(new Vector2());
			this.Vectors.Add(new Vector2());
			this.Vectors.Add(new Vector2());
		}

		public Rect Transform(Matrix3x2 m)
		{
			Rect r2 = new Rect();
			r2.V1 = Vector2.Transform(this.V1, m);
			r2.V2 = Vector2.Transform(this.V2, m);
			r2.V3 = Vector2.Transform(this.V3, m);
			r2.V4 = Vector2.Transform(this.V4, m);
			return r2;
		}
	}
}