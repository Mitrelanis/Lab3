using System;
using System.Threading.Tasks;

public class Matrix
{
    private double[,] _values;

    public Matrix(double[,] initialValues)
    {
        _values = initialValues;
    }

    public static Matrix Zero(int r, int c){
        return new Matrix(new double[r,c]);
    }

    public static Matrix Identity(int n){
        Matrix result = Zero(n, n);
        for (int i = 0; i < n; i++)
        {
            result._values[i, i] = 1;
        }
        return result;
    }

    public static Matrix Zero(int n){
        return Zero(n, n);
    }

    public double this[int i, int j]{
        get{
            return _values[i, j];
        }
    }

    public int Rows{
        get{
            return _values.GetLength(0);
        }
    }

    public int Columns{
        get{
            return _values.GetLength(1);
        }
    }

    public Matrix Transpose(){
        var transposed = new double[Columns, Rows];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                transposed[j, i] = _values[i, j];
            }
        }
        return new Matrix(transposed);
    }

    public override string ToString(){
        string result = "";
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result += _values[i, j] + " ";
            }
            result += "\n";
        }
        return result;
    }
    
    public override bool Equals(object obj)
    {
        if (obj is Matrix other){
            if (Rows != other.Rows || Columns != other.Columns){
                return false;
            }
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (_values[i, j] != other[i, j]){
                        return false;
                    }
                }
            }
            return true;
        }
        return false;
    }

    public override int GetHashCode(){
        return _values.GetHashCode();
    }

    public static Matrix operator +(Matrix a, Matrix b){
        return MatrixOperations.Add(a, b);
    }

    public static Matrix operator -(Matrix a, Matrix b){
        return MatrixOperations.Subtract(a, b);
    }

    public static Matrix operator *(Matrix a, double scalar){
        return MatrixOperations.ScalarMultiply(a, scalar);
    }

    public static Matrix operator ~(Matrix a){
        return a.Transpose();
    }

    public static Matrix operator +(Matrix a){
        return a;
    }

    public static Matrix operator -(Matrix a){
        return a * -1;
    }

    public static Matrix operator *(Matrix a, Matrix b)
    {
        return MatrixOperations.Multiply(a, b);
    }
}
