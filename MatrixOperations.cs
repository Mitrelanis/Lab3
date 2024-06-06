using System;
using System.Threading.Tasks;

public static class MatrixOperations{

    public static Matrix Transpose(Matrix m){
        var transposed = new double[m.Columns, m.Rows];
        for (int i = 0; i < m.Rows; i++)
        {
            for (int j = 0; j < m.Columns; j++){
                transposed[j, i] = m[i, j];
            }
        }
        return new Matrix(transposed);
    }

    public static Matrix ScalarMultiply(Matrix a, double scalar){
        var result = new double[a.Rows, a.Columns];
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < a.Columns; j++)
            {
                result[i, j] = a[i, j] * scalar;
            }
        }
        return new Matrix(result);

    }

    public static Matrix Add(Matrix a, Matrix b){
        if (a.Rows != b.Rows || a.Columns != b.Columns){
            throw new InvalidOperationException("Matrices must have the same dimensions.");
        }
        else{
            var result = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }
            return new Matrix(result);
        }
    }

    public static Matrix Subtract(Matrix a, Matrix b){
        if (a.Rows != b.Rows || a.Columns != b.Columns){
            throw new InvalidOperationException("Matrices must have the same dimensions.");
        }
        else{
            var result = new double[a.Rows, a.Columns];
            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }
            return new Matrix(result);
        }
    }

    public static Matrix Multiply(Matrix a, Matrix b){
        if (a.Columns != b.Rows)
        {
            throw new InvalidOperationException("The number of columns in the first matrix must be equal to the number of rows in the second matrix.");
        }
        else
        {
            Matrix bTransposed = b.Transpose();
            var result = new double[a.Rows, b.Columns];

            Parallel.For(0, a.Rows, i =>
            {
                for (int j = 0; j < b.Columns; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < a.Columns; k++)
                    {
                        sum += a[i, k] * bTransposed[j, k];
                    }
                    result[i, j] = sum;
                }
            });

            return new Matrix(result);
        }
    }


}