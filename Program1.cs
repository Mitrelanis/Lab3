using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program{
    public static async Task Main(string[] args){
        Matrix[] a = new Matrix[50];
        Matrix[] b = new Matrix[50];
        for (int i = 0; i < 50; i++)
        {
            a[i] = CreateRandomMatrix(500, 100);
            b[i] = CreateRandomMatrix(100, 500);
        }

        DirectoryInfo resdirectory = CreateAndClearDirectory("resdirectory");

        Task calcTask = Task.Run(async() => {

            string sep = " ";
            string extension = ".tsv";

            var saveResult = async (Task<Matrix> task, string name) =>
            {
                var result = await task;
                Console.WriteLine($"{name} calculation is finished");

                await MatrixIO.WriteMatrixToFileAsync(
                        resdirectory,
                        name + extension,
                        result,
                        (Matrix matrix, Stream stream) => MatrixIO.WriteMatrixToTextAsync(stream, matrix, sep)
                    );
            };

            Task[] tasks = {
                saveResult(Task.Run(() => MultiplyMatrices(a, b)), "MultiplyMatrices_ab"),
                saveResult(Task.Run(() => MultiplyMatrices(b, a)), "MultiplyMatrices_ba"),
                saveResult(Task.Run(() => MultiplyMatricesScalar(a, b)), "MultiplyMatricesScalar_ab"),
                saveResult(Task.Run(() => MultiplyMatricesScalar(b, a)), "MultiplyMatricesScalar_ba")
            };

            

            await Task.WhenAll(tasks);

        });
        
        DirectoryInfo csvdirectory = CreateAndClearDirectory("csvdirectory");
        DirectoryInfo jsondirectory = CreateAndClearDirectory("jsondirectory");
        DirectoryInfo bindirectory = CreateAndClearDirectory("bindirectory");

        Task writeAsyncTask = Task.Run(async () =>
        {
            string aPrefix = "a";
            string bPrefix = "b";

            string textExtension = "csv";
            string jsonExtension = "json";

            string textSep = ";";


            Task aCsvTask = WriteMatrixArrayAsync(
                csvdirectory,
                aPrefix,
                textExtension,
                a,
                (matrix, stream) => MatrixIO.WriteMatrixToTextAsync(stream, matrix,  textSep)
            );

            Task bCsvTask = WriteMatrixArrayAsync(
                csvdirectory,
                bPrefix,
                textExtension,
                b,
                (matrix, stream) => MatrixIO.WriteMatrixToTextAsync(stream, matrix,  textSep)
            );

            Task aJsonTask = WriteMatrixArrayAsync(
                jsondirectory,
                aPrefix,
                jsonExtension,
                a,
                (matrix, stream) => MatrixIO.WriteMatrixToJsonAsync(stream, matrix)
            );

            Task bJsonTask = WriteMatrixArrayAsync(
                jsondirectory,
                bPrefix,
                jsonExtension,
                b,
                (matrix, stream) => MatrixIO.WriteMatrixToJsonAsync(stream, matrix)
            );

            await Task.WhenAll(aCsvTask, bCsvTask, aJsonTask, bJsonTask);

            Console.WriteLine("Write async finished");

            Task<Matrix[]> csvRead = ReadMatrixArrayAsync(
                csvdirectory,
                aPrefix,
                textExtension,
                (stream) => MatrixIO.ReadMatrixFromTextAsync(stream, textSep)
            );

            Task<Matrix[]> jsonRead = ReadMatrixArrayAsync(
                jsondirectory,
                aPrefix,
                jsonExtension,
                (stream) => MatrixIO.ReadMatrixJsonAsync(stream)
            );

            
            var readTasks = new List<Task<Matrix[]>> { csvRead, jsonRead };

            Matrix[] csvA=new Matrix[a.Length];
            Matrix[] jsonA=new Matrix[a.Length];

            while (readTasks.Count > 0)
            {
                var finished = await Task.WhenAny(readTasks);

                var result = await finished;
                readTasks.Remove(finished);

                if (finished == csvRead)
                {
                    Console.WriteLine("Csv finished");
                    csvA = result;
                }
                else
                {
                    Console.WriteLine("Json finished");
                    jsonA = result;
                }
            }

            Task<bool> compareTask1 = Task.Run(() => MatrixEquals(a, csvA));
            Task<bool> compareTask2 = Task.Run(() => MatrixEquals(a, jsonA));

            Task.WhenAll(compareTask1, compareTask2).ContinueWith(task =>
            {
                Console.WriteLine($"Comparison of a and csvA: {compareTask1.Result}");
                Console.WriteLine($"Comparison of a and jsonA: {compareTask2.Result}");
            }).Wait();

            Console.WriteLine("All comparisons are completed.");

            
        });

        {
            Console.WriteLine("Write started");
            string aPrefix = "a";
            string bPrefix = "b";

            WriteMatrixArray(
                bindirectory,
                aPrefix,
                "bin",
                a,
                (matrix, stream) => MatrixIO.WriteMatrixToBinary(stream, matrix)

            );

            WriteMatrixArray(
                bindirectory,
                bPrefix,
                "bin",
                b,
                (matrix, stream) => MatrixIO.WriteMatrixToBinary(stream, matrix)

            );

            Console.WriteLine("Write finished");

            var binA = ReadMatrixArray(
                bindirectory,
                aPrefix,
                "bin",
                (stream) => MatrixIO.ReadMatrixFromBinary(stream)
            );

            var eq = Equals(a, binA);
            Console.WriteLine($"Binary a equals: {eq}");
        }
    
        await Task.WhenAll(calcTask, writeAsyncTask);



    }
    private static DirectoryInfo CreateAndClearDirectory(string path)
    {
        DirectoryInfo directory = new DirectoryInfo(path);
        if (directory.Exists)
        {
            directory.Delete(true);
        }
        directory.Create();
        return directory;
    }

    static Random random = new Random();

    public static Matrix CreateRandomMatrix(int rows, int columns){
        var matrix = new double[rows, columns];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                matrix[i, j] = random.Next(-10, 10);
            }
        }
        return new Matrix(matrix);
    }

    public static Matrix MultiplyMatrices(Matrix[] a, Matrix[] b){
        if (a.Length != b.Length){
            throw new InvalidOperationException("Arrays must have the same length.");
        }
        Matrix result = Matrix.Zero(a[0].Rows, a[0].Columns);
        result += a[0];
        result *= b[0];
        for (int i = 1; i < a.Length; i++)
        {
            result = result * a[i];
            result = result * b[i];
        }
        return result;
    }

    public static Matrix MultiplyMatricesScalar(Matrix[] a, Matrix[] b){
        if (a.Length != b.Length){
            throw new InvalidOperationException("Arrays must have the same length.");
        }
        Matrix result = Matrix.Zero(a[0].Rows, b[0].Columns);
        for (int i = 0; i < a.Length; i++)
        {
            result += a[i]*b[i];
        }
        return result;
    }

    public static void WriteMatrixArray(DirectoryInfo directory, string filePrefix, string fileExtension, Matrix[] matrices, Action<Matrix, Stream> writeMethod)
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            MatrixIO.WriteMatrixToFile(directory, $"{filePrefix}{i}.{fileExtension}", matrices[i], writeMethod);
            if (i % 10 == 9)
            {
                Console.WriteLine($"Wrote {filePrefix}{i}.{fileExtension}");
            }
        }
    }

    public static async Task WriteMatrixArrayAsync(DirectoryInfo directory, string filePrefix, string fileExtension, Matrix[] matrices, Func<Matrix, Stream, Task> writeMethod)
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            await MatrixIO.WriteMatrixToFileAsync(directory, $"{filePrefix}{i}.{fileExtension}", matrices[i], writeMethod);
            if (i % 10 == 9)
            {
                Console.WriteLine($"Wrote {filePrefix}{i}.{fileExtension} async");
            }
        }
    }

    public static Matrix[] ReadMatrixArray(DirectoryInfo directory, string filePrefix, string fileExtension, Func<Stream, Matrix> readMethod)
    {
        var filePaths = directory.GetFiles( $"{filePrefix}*.{fileExtension}");
        var matrices = new Matrix[filePaths.Length];
        foreach (var filePath in filePaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath.Name);
            var index = int.Parse(fileName.Substring(filePrefix.Length));
            matrices[index] = MatrixIO.ReadMatrixFromFile(filePath, readMethod);
        }
        return matrices;
    }

    public static async Task<Matrix[]> ReadMatrixArrayAsync(DirectoryInfo directory, string filePrefix, string fileExtension, Func<Stream, Task<Matrix>> readMethod)
    {
        var filePaths = directory.GetFiles( $"{filePrefix}*.{fileExtension}");
        var matrices = new Matrix[filePaths.Length];
        foreach (var filePath in filePaths)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath.Name);
            var index = int.Parse(fileName.Substring(filePrefix.Length));
            matrices[index] = await MatrixIO.ReadMatrixFromFileAsync(filePath, readMethod);
        }
        return matrices;
    }
    
    public static bool MatrixEquals(Matrix[] a, Matrix[] b){
        if (a.Length != b.Length){
            return false;
        }
        for (int i = 0; i < a.Length; i++)
        {
            if (!a[i].Equals(b[i])){
                return false;
            }
        }
        return true;
    }

    
}
