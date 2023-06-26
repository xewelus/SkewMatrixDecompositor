#nullable enable
using UnityEngine;
using Matrix3x2 = System.Numerics.Matrix3x2;

namespace SkewMatrixDecompositor
{
	public class MatrixObjectWrapper
	{
		public readonly GameObject Target;
		public readonly GameObject OuterLayer;
		private GameObject? layer1;
		private GameObject? layer2;
		private Matrix3x2 prevMatrix;
		private static readonly Quaternion zeroQuaternion = Quaternion.Euler(0f, 0f, 0f);
		private static readonly Vector3 one = new Vector3(1f, 1f);

		public MatrixObjectWrapper(GameObject target, Matrix3x2 matrix)
		{
			this.Target = target;

			this.OuterLayer = new GameObject($"{this.Target.name} outer layer");
			this.OuterLayer.transform.parent = target.transform.parent;

			this.Update(matrix);
		}

		public void Update(Matrix3x2 matrix)
		{
			if (this.prevMatrix == matrix) return;
			this.prevMatrix = matrix;

			if (this.layer1 != null)
			{
				this.layer1.SetActive(false);
			}
			if (this.layer2 != null)
			{
				this.layer2.SetActive(false);
			}

			GameObject lastLayer = this.OuterLayer;

			MatrixDecompositor.Result? result = MatrixDecompositor.Decompose(matrix);
			if (result == null)
			{
				this.Target.transform.SetParent(this.OuterLayer.transform, false);
				return;
			}

			if (result.Scale2 != null || result.Rotate2 != null || result.Translate != null)
			{
				if (this.layer2 == null)
				{
					this.layer2 = new GameObject($"{this.Target.name} layer 2");
				}
				else
				{
					this.layer2.SetActive(true);
				}

				this.layer2.transform.SetParent(lastLayer.transform, false);

				if (result.Scale2 == null)
				{
					this.layer2.transform.localScale = one;
				}
				else
				{
					this.layer2.transform.localScale = new Vector3(result.Scale2.Value.x, result.Scale2.Value.y);
				}

				if (result.Rotate2 == null)
				{
					this.layer2.transform.localRotation = zeroQuaternion;
				}
				else
				{
					this.layer2.transform.localRotation = Quaternion.Euler(0f, 0f, -result.Rotate2.Value * Mathf.Rad2Deg);
				}

				if (result.Translate == null)
				{
					this.layer2.transform.localPosition = Vector3.zero;
				}
				else
				{
					this.layer2.transform.localPosition = new Vector3(result.Translate.Value.x, result.Translate.Value.y);
				}

				lastLayer = this.layer2;
			}

			if (result.Rotate1 != null || result.Scale1 != null)
			{
				if (this.layer1 == null)
				{
					this.layer1 = new GameObject($"{this.Target.name} layer 1");
				}
				else
				{
					this.layer1.SetActive(true);
				}
				this.layer1.transform.SetParent(lastLayer.transform, false);

				if (result.Scale1 == null)
				{
					this.layer1.transform.localScale = one;
				}
				else
				{
					this.layer1.transform.localScale = new Vector3(result.Scale1.Value.x, result.Scale1.Value.y);
				}

				if (result.Rotate1 == null)
				{
					this.layer1.transform.localRotation = zeroQuaternion;
				}
				else
				{
					this.layer1.transform.localRotation = Quaternion.Euler(0f, 0f, -result.Rotate1.Value * Mathf.Rad2Deg);
				}

				lastLayer = this.layer1;
			}

			this.Target.transform.SetParent(lastLayer.transform, false);
		}

		public void Destroy(bool destroyTarget = true)
		{
			if (this.layer2 != null)
			{
				Object.Destroy(this.layer2);
				this.layer2 = null;
			}
			if (this.layer1 != null)
			{
				Object.Destroy(this.layer1);
				this.layer1 = null;
			}
			Object.Destroy(this.OuterLayer);

			if (destroyTarget)
			{
				Object.Destroy(this.Target);
			}
		}
	}
}
