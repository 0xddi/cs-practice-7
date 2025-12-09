namespace App;

public class Misc
{
    public static int CountLines(FileInfo filePath)
    {
        int countLines = 0;

        using (StreamReader reader = new StreamReader(filePath.FullName))
        {
            while (reader.ReadLine() != null)
            {
                countLines++;
            }
        }
        return countLines;
    }

    public static async Task CleanUpOnCancellation(FileInfo file, FileStream fileStream)
    {
        // Этот блок выполнится при отмене Parallel.ForEachAsync
        Console.WriteLine("[-] Операция отменена");
        
        try
        {
            // Закрываем поток перед удалением
            await fileStream.DisposeAsync();
                
                
            // Несколько попыток удаления
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    file.Delete();
                    Console.WriteLine($"[+] Файл {file.FullName} удален");
                    return; // Выходим из метода, а потом из программы
                }
                catch (IOException) when (i < 2) // Если файл еще используется
                { 
                    await Task.Delay(300);
                }
            }
            Console.WriteLine($"[-] Не удалось удалить файл {file.FullName}, удалите его вручную");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Не удалось удалить файл: {ex.Message}");
        }
    }
}