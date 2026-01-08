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
        // Этот блок выполнится при отмене в Parallel.ForEachAsync
        Console.WriteLine("[-] Операция отменена");
        
        try
        {
            // Закрываем поток перед удалением
            await fileStream.DisposeAsync();
            
            try
            {
                file.Delete();
                Console.WriteLine($"[+] Файл {file.FullName} удален");
            }
            catch (IOException) // Если файл еще используется
            { 
                Console.WriteLine($"[-] Не удалось удалить файл {file.FullName}, удалите его вручную");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Не удалось удалить файл: {ex.Message}");
        }
    }
}