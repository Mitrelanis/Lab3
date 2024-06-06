using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;


public static class MatrixIO{
    public static async Task WriteMatrixToTextAsync(Stream stream, Matrix matrix, string sep = " ")
    {
        using (var writer = new StreamWriter(stream))
        {
            await writer.WriteLineAsync($"{matrix.Rows}{sep}{matrix.Columns}");
            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    await writer.WriteAsync(matrix[i, j].ToString());
                    if (j < matrix.Columns - 1)
                        await writer.WriteAsync(sep);
                }
                await writer.WriteLineAsync();
            }
        }
    }

    public static async Task<Matrix> ReadMatrixFromTextAsync(Stream stream, string sep = " ")
    {
        using (var reader = new StreamReader(stream))
        {
            var size = await reader.ReadLineAsync();
            var parts = size.Split(sep);
            int rows = int.Parse(parts[0]);
            int cols = int.Parse(parts[1]);

            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                var line = await reader.ReadLineAsync();
                var numbers = line.Split(sep);
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = double.Parse(numbers[j]);
                }
            }

            return new Matrix(matrix);
        }
    }

    public static void WriteMatrixToBinary(Stream stream, Matrix matrix)
    {
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(matrix.Rows);
            writer.Write(matrix.Columns);

            for (int i = 0; i < matrix.Rows; i++)
            {
                for (int j = 0; j < matrix.Columns; j++)
                {
                    writer.Write(matrix[i, j]);
                }
            }
        }
    }

    public static Matrix ReadMatrixFromBinary(Stream stream)
    {
        using (var reader = new BinaryReader(stream))
        {
            int rows = reader.ReadInt32();
            int cols = reader.ReadInt32();

            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = reader.ReadDouble();
                }
            }

            return new Matrix(matrix);
        }
    }

    public static async Task WriteMatrixToJsonAsync(Stream stream, Matrix matrix)
    {
        double[][] jaggedMatrix = new double[matrix.Rows][];
        for (int i = 0; i < matrix.Rows; i++)
        {
            jaggedMatrix[i] = new double[matrix.Columns];
            for (int j = 0; j < matrix.Columns; j++)
                jaggedMatrix[i][j] = matrix[i, j];
        }
        
        await JsonSerializer.SerializeAsync(stream, jaggedMatrix);
    }

    public static async Task<Matrix> ReadMatrixJsonAsync(Stream stream)
    {
        var jaggedMatrix = await JsonSerializer.DeserializeAsync<double[][]>(stream);
        int rows = jaggedMatrix.Length;
        int cols = jaggedMatrix[0].Length;
        double[,] matrix = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = jaggedMatrix[i][j];
            }
        }

        return new Matrix(matrix);
    }
    
    public static void WriteMatrixToFile(DirectoryInfo directory, string fileName, Matrix matrix, Action<Matrix, Stream> writeMethod)
    {
        using (var stream = File.OpenWrite(Path.Combine(directory.FullName, fileName)))
        {
            writeMethod(matrix, stream);
        }
    }

    public static async Task WriteMatrixToFileAsync(DirectoryInfo directory, string fileName, Matrix matrix, Func<Matrix, Stream, Task> writeMethod)
    {
        using (var stream = File.OpenWrite(Path.Combine(directory.FullName, fileName)))
        {
            await writeMethod(matrix, stream);
        }
    }

    public static Matrix ReadMatrixFromFile(FileInfo filePath, Func<Stream, Matrix> readMethod)
    {
        using (var stream = filePath.OpenRead())
        {
            return readMethod(stream);
        }
    }

    public static async Task<Matrix> ReadMatrixFromFileAsync(FileInfo filePath, Func<Stream, Task<Matrix>> readMethod)
    {
        using (var stream = filePath.OpenRead())
        {
            return await readMethod(stream);
        }
    }       


}