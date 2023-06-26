# SkewMatrixDecompositor
This class solves the problem of decomposition of an arbitrary matrix into a series of primitive matrices, which are rotations, scaling and translation. Solves the problem of the lack of mechanisms for working with the skew-component of the matrix, for example, in Unity.

![skewed](https://github.com/xewelus/SkewMatrixDecompositor/assets/1760365/c46a317c-eec3-4df3-b05f-b0cf977d52af)

This repository contains two implementations of matrix transformations - for **Unity** and for **.NET Core**.

## Unity ##

1. Copy the files from the **MatrixDecompositorUnity\Assets\SkewMatrixTest** folder to your Unity project.

2. There is a ```MatrixObjectWrapper``` class that turns the desired ```GameObject``` into a series of nested layers (```GameObject```), to which the necessary transformations are applied.
   Using example (from ```RootScript```):

```
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
```

4. For a quick health check
   
    3.1 Add an empty ```GameObject``` to the stage

    3.2 Add a ```RootScript``` component to it

    3.3 Assign in the editor to the ```Target``` field the object to which the matrix will be applied

    3.4 In the ```Matrix``` field set the parameters of the matrix.

    3.5 After starting the scene, layers will be created in the editor and the ```Target``` object will move into them

    3.6 You can change the values in the ```Matrix``` field online

5. If you just need to use the decomposition of the matrix into components, then you can use the method:
   
```MatrixDecompositor.Result? MatrixDecompositor.Decompose(Matrix3x2 matrix)```

Description of the returned Result:

```
public class Result
{
    public UnityEngine.Vector2? Scale1;
    public float? Rotate1;
    public UnityEngine.Vector2? Scale2;
    public float? Rotate2;
    public UnityEngine.Vector2? Translate;
}
```

## .NET Core ##

There are two methods implemented that can be useful in different situations:

1. ```MatrixDecompositor.Result? MatrixDecompositor.GetDecomposeResult(Matrix3x2 matrix)``` - see above.

2. ```List<Matrix3x2>? MatrixDecompositor.Decompose(Matrix3x2 matrix)``` - decomposes the matrix into a series of simple matrices, multiplying which, we get the original matrix.
