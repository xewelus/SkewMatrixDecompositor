using System.Numerics;

namespace SkewMatrixDecompositor
{
	internal class MatrixChain
	{
		public bool UseRect2;

		private readonly Rect rect1;
		private readonly Rect rect2;

		public MatrixChain(Rect rect)
		{
			this.rect1 = rect;

			this.rect2 = new Rect();
			this.rect2.V1 = rect.V4;
			this.rect2.V2 = rect.V1;
			this.rect2.V3 = rect.V2;
			this.rect2.V4 = rect.V3;
		}

		public Rect Rect
		{
			get
			{
				return this.UseRect2 ? this.rect2 : this.rect1;
			}
		}

		public MatrixChain Transform(Matrix3x2 next)
		{
			Rect rect = this.rect1.Transform(next);

			MatrixChain c = new MatrixChain(rect);
			c.UseRect2 = this.UseRect2;
			return c;
		}
	}
}