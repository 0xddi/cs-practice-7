using System.Runtime.CompilerServices;

namespace App;

/// <summary>
/// Считывает и валидирует ввод пользователя.
/// </summary>
public static class Input
{
    /// <summary>
    /// Считывает от пользователя URL файлов из интернета.
    /// </summary>
    public static string[] GetUris()
    {
        string[] result = [];
        var valid = false;
        while (!valid)
        {
            Console.Write("Введите нужные URL через пробел: ");
            result = Console.ReadLine()!.Split(' ');
            valid = !result.Any(x => IsValidUri(x) is false);
            if (valid is false)
            {
                Console.WriteLine("[-] Ошибка! Вы ввели некорректные URL! Попробуйте снова");
            }
        }

        return result;
    }

    private static bool IsValidUri(string uri) => Uri.TryCreate(uri, UriKind.Absolute, out _) && uri.StartsWith("https://");

    private static bool IsValidPath(string path)
    {
        try
        {
            // Создаем файл, но сразу закрываем его через using
            using (var file = File.Create(path))
            {
                
            }
        
            // Теперь файл закрыт и мы можем его удалить
            File.Delete(path);
            return true;
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine(
                $"[-] Ошибка при валидации пути. Скорее всего одной из директорий указанных в пути не существует: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[-] Неизвестная ошибка при валидации пути: {ex.Message}");
            return false;
        }
    }
    
    
    /// <summary>
    /// Считывает от пользователя путь до файла с результатом.
    /// </summary>
    public static FileInfo GetPathToOutputFile()
    {
        FileInfo file;
        while (true)
        {
            Console.Write("Введите путь до файла с результатом: ");
            file = new FileInfo(Console.ReadLine()!.Trim('"'));
            try
            {
                if (file.Exists)
                {
                    Console.WriteLine("[?] Файл уже существует. Хотите ли вы его перезаписать? [y/n]");
                    Console.Write(">>");
                    var answer = Console.ReadLine()!.ToLower();
                    if (answer == "y")
                    {
                        Console.WriteLine("[+] Вы согласились на перезапись файла.");
                        File.Delete(file.FullName);
                        return file;
                    }

                    Console.WriteLine("[-] Вы отказались от перезаписи. Вернёмся в начало.");
                    continue;
                }
                if (!IsValidPath(file.FullName))
                {
                    Console.WriteLine("[-] Путь к файлу некорректен. Попробуйте снова");
                    continue;
                }

                return file;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[-] I/O ошибка при чтении {file.FullName}: {ex.Message}. Скорее всего файл используется другим процессом.");
            }

            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[-] Ошибка доступа при чтении {file.FullName}: {ex.Message}. Скорее всего дело в правах пользователя.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] Неизвестная ошибка при чтении {file.FullName}: {ex.Message}.");
            }
        }
    }
}